using CodeWalker.GameFiles;
using CodeWalker.Utils;
using CodeWalker.World;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace CodeWalker.Project.Panels
{
    public partial class PaintPropsPanel : ProjectPanel
    {
        public ProjectForm ProjectForm { get; set; }
        public ProjectFile CurrentProjectFile { get; set; }

        private Random Rand = new Random();

        public PaintPropsPanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();

            if (ProjectForm?.WorldForm == null)
            {
                //could happen in some other startup mode - world form is required for this..
                PaintEnabledCheckBox.Enabled = false;
            }

            this.DockStateChanged += onDockStateChanged;

            UpdateModeControls();
            UpdateTargetLabel();
        }

        private void onDockStateChanged(object sender, EventArgs e)
        {
            if (DockState == DockState.Hidden)
            {
                PaintEnabledCheckBox.Checked = false;
            }
        }

        public void SetProject(ProjectFile project)
        {
            CurrentProjectFile = project;
            UpdateTargetLabel();
        }

        internal bool IsGrassMode => ModeGrassRadio.Checked;
        internal bool PaintEnabled
        {
            get
            {
                if (!PaintEnabledCheckBox.Checked) return false;
                return IsGrassMode || (PropsListBox.Items.Count > 0);
            }
        }
        internal float BrushRadius => (float)RadiusNumericUpDown.Value;


        //######## Keybind actions (called by the world view)

        public void TogglePaintEnabled()
        {
            if (!PaintEnabledCheckBox.Enabled) return;
            PaintEnabledCheckBox.Checked = !PaintEnabledCheckBox.Checked;
            SetStatus(PaintEnabledCheckBox.Checked ? "Painting enabled - hold CTRL and drag in the world view." : "Painting disabled.");
        }
        public void ToggleMode()
        {
            if (IsGrassMode) ModePropsRadio.Checked = true;
            else ModeGrassRadio.Checked = true;
            SetStatus(IsGrassMode ? "Grass mode" : "Prop mode");
        }
        public void AdjustBrushRadius(float factor)
        {
            decimal v = RadiusNumericUpDown.Value;
            v = Math.Round(v * (decimal)factor, 1);
            if (factor > 1.0f && v == RadiusNumericUpDown.Value) v += RadiusNumericUpDown.Increment;
            if (factor < 1.0f && v == RadiusNumericUpDown.Value) v -= RadiusNumericUpDown.Increment;
            RadiusNumericUpDown.Value = Math.Max(RadiusNumericUpDown.Minimum, Math.Min(RadiusNumericUpDown.Maximum, v));
        }


        //######## Painting

        public void PaintAtMouse(SpaceRayIntersectResult mouseRay, bool erase)
        {
            var wf = ProjectForm?.WorldForm;
            if (wf == null) return;
            if (!mouseRay.Hit) return;

            var ymap = ProjectForm.GetPaintPropsTargetYmap();
            UpdateTargetLabel(ymap);
            if (ymap == null)
            {
                SetStatus("Select a ymap in the project first (Ymap > New Ymap File to make one).");
                return;
            }

            if (erase)
            {
                Erase(ymap, mouseRay.Position);
                return;
            }

            if (IsGrassMode) PaintGrass(wf, ymap, mouseRay);
            else PaintProps(wf, ymap, mouseRay);
        }

        private void Erase(YmapFile ymap, Vector3 pos)
        {
            if (IsGrassMode)
            {
                int removed = ProjectForm.EraseGrassInstances(ymap, pos, BrushRadius);
                if (removed > 0) SetStatus("Erased " + removed.ToString() + " grass blade(s) from " + ymap.Name);
                return;
            }

            var hashes = new HashSet<uint>();
            foreach (var item in PropsListBox.Items)
            {
                hashes.Add(JenkHash.GenHash(item.ToString().ToLowerInvariant()));
            }
            int n = ProjectForm.ErasePaintedEntities(ymap, pos, BrushRadius, hashes);
            if (n > 0) SetStatus("Erased " + n.ToString() + " prop(s) from " + ymap.Name);
        }

        private class Placement
        {
            public Vector3 Position;
            public Vector3 Normal;
        }

        private List<Placement> FindPlacements(WorldForm wf, YmapFile ymap, SpaceRayIntersectResult mouseRay, float spacing, Func<Vector3, bool> blocked)
        {
            float radius = BrushRadius;
            int count = (int)CountNumericUpDown.Value;
            float minSlopeCos = (float)Math.Cos(MathUtil.DegreesToRadians((float)MaxSlopeNumericUpDown.Value));
            float zOffset = (float)ZOffsetNumericUpDown.Value;

            var res = new List<Placement>();

            //Space.RayIntersect reuses the shared MapDataStore/BoundsStore VisibleItems lists, which the
            //render thread also iterates each frame, so the raycasts must happen inside the render lock.
            lock (wf.RenderSyncRoot)
            {
                for (int i = 0; i < count; i++)
                {
                    //uniform random point in the brush circle, then drop it onto the ground below.
                    float r = radius * (float)Math.Sqrt(Rand.NextDouble());
                    float a = (float)(Rand.NextDouble() * Math.PI * 2.0);
                    var p = mouseRay.Position + new Vector3((float)Math.Cos(a) * r, (float)Math.Sin(a) * r, 0);
                    var ray = new Ray(p + new Vector3(0, 0, radius + 2.0f), -Vector3.UnitZ);
                    var hit = wf.Raycast(ray);
                    if (!hit.Hit) continue;
                    if (Math.Abs(hit.Position.Z - mouseRay.Position.Z) > (radius * 2.0f + 5.0f)) continue; //probably hit a roof or fell off a cliff

                    var normal = Vector3.Normalize(hit.Normal);
                    if (normal.Z < minSlopeCos) continue; //too steep

                    var pos = hit.Position + new Vector3(0, 0, zOffset);

                    bool tooclose = false;
                    float sq = spacing * spacing;
                    if (spacing > 0)
                    {
                        foreach (var o in res)
                        {
                            if (Vector3.DistanceSquared(o.Position, pos) < sq) { tooclose = true; break; }
                        }
                    }
                    if (tooclose) continue;
                    if ((blocked != null) && blocked(pos)) continue;

                    res.Add(new Placement { Position = pos, Normal = normal });
                }
            }
            return res;
        }

        private void PaintProps(WorldForm wf, YmapFile ymap, SpaceRayIntersectResult mouseRay)
        {
            var archetypes = GetPaletteArchetypes();
            if (archetypes.Count == 0)
            {
                SetStatus("None of the props in the list were found in the game files.");
                return;
            }

            float spacing = (float)SpacingNumericUpDown.Value;
            float scaleMin = (float)ScaleMinNumericUpDown.Value;
            float scaleMax = Math.Max(scaleMin, (float)ScaleMaxNumericUpDown.Value);
            bool randomYaw = RandomYawCheckBox.Checked;
            bool alignToSurface = AlignToSurfaceCheckBox.Checked;

            Func<Vector3, bool> blocked = (pos) => IsNearExistingEntity(ymap, pos, spacing);
            var placements = FindPlacements(wf, ymap, mouseRay, spacing, blocked);

            int created = 0;
            foreach (var pl in placements)
            {
                var arch = archetypes[Rand.Next(archetypes.Count)];
                float yaw = randomYaw ? (float)(Rand.NextDouble() * Math.PI * 2.0) : 0.0f;
                var ori = GetOrientation(alignToSurface ? pl.Normal : Vector3.UnitZ, yaw);
                float scale = scaleMin + (float)Rand.NextDouble() * (scaleMax - scaleMin);

                var ent = ProjectForm.PaintEntity(ymap, arch, pl.Position, ori, scale);
                if (ent != null) created++;
            }

            if (created > 0)
            {
                SetStatus("Painted " + created.ToString() + " prop(s) into " + ymap.Name + " (" + (ymap.AllEntities?.Length ?? 0).ToString() + " entities)");
            }
        }

        private void PaintGrass(WorldForm wf, YmapFile ymap, SpaceRayIntersectResult mouseRay)
        {
            string typeName = GrassTypeTextBox.Text?.Trim();
            if (string.IsNullOrEmpty(typeName))
            {
                SetStatus("Enter a grass type (e.g. proc_grasses01).");
                return;
            }
            uint archHash = JenkHash.GenHash(typeName.ToLowerInvariant());

            float spacing = (float)SpacingNumericUpDown.Value;
            var placements = FindPlacements(wf, ymap, mouseRay, spacing, null);
            if (placements.Count == 0) return;

            int smin = (int)GrassScaleMinNumericUpDown.Value;
            int smax = Math.Max(smin, (int)GrassScaleMaxNumericUpDown.Value);

            var positions = new List<Vector3>();
            var normals = new List<Vector3>();
            var scales = new List<byte>();
            foreach (var pl in placements)
            {
                positions.Add(pl.Position);
                normals.Add(pl.Normal);
                scales.Add((byte)Rand.Next(smin, smax + 1));
            }

            var c = GrassColorLabel.BackColor;
            var col = new SharpDX.Color(c.R, c.G, c.B);
            int added = ProjectForm.PaintGrassInstances(ymap, archHash, positions, normals, scales, col, (byte)GrassAoNumericUpDown.Value, BrushRadius);
            if (added > 0)
            {
                SetStatus("Painted " + added.ToString() + " grass blade(s) into " + ymap.Name);
            }
        }


        private List<Archetype> GetPaletteArchetypes()
        {
            var list = new List<Archetype>();
            var gfc = ProjectForm?.GameFileCache;
            if (gfc == null) return list;
            foreach (var item in PropsListBox.Items)
            {
                var name = item.ToString();
                var arch = gfc.GetArchetype(JenkHash.GenHash(name.ToLowerInvariant()));
                if (arch != null) list.Add(arch);
            }
            return list;
        }

        private bool IsNearExistingEntity(YmapFile ymap, Vector3 pos, float spacing)
        {
            if (spacing <= 0) return false;
            float sq = spacing * spacing;
            if (ymap.AllEntities != null)
            {
                foreach (var ent in ymap.AllEntities)
                {
                    if (Vector3.DistanceSquared(ent.Position, pos) < sq) return true;
                }
            }
            return false;
        }

        private static Quaternion GetOrientation(Vector3 up, float yaw)
        {
            //build a basis with Z = up and Y = the yaw heading projected onto the surface plane.
            var heading = new Vector3(-(float)Math.Sin(yaw), (float)Math.Cos(yaw), 0);
            var forward = heading - up * Vector3.Dot(heading, up);
            if (forward.LengthSquared() < 1e-6f)
            {
                forward = Vector3.UnitY;
            }
            forward = Vector3.Normalize(forward);
            var right = Vector3.Normalize(Vector3.Cross(forward, up));
            forward = Vector3.Cross(up, right);

            var m = Matrix.Identity;
            m.M11 = right.X; m.M12 = right.Y; m.M13 = right.Z;
            m.M21 = forward.X; m.M22 = forward.Y; m.M23 = forward.Z;
            m.M31 = up.X; m.M32 = up.Y; m.M33 = up.Z;
            return Quaternion.Normalize(Quaternion.RotationMatrix(m));
        }

        private void AddProp(string name)
        {
            name = name?.Trim();
            if (string.IsNullOrEmpty(name)) return;
            foreach (var item in PropsListBox.Items)
            {
                if (string.Equals(item.ToString(), name, StringComparison.OrdinalIgnoreCase)) return;
            }

            var gfc = ProjectForm?.GameFileCache;
            if ((gfc != null) && (gfc.GetArchetype(JenkHash.GenHash(name.ToLowerInvariant())) == null))
            {
                SetStatus("Warning: archetype \"" + name + "\" wasn't found in the loaded game files.");
            }
            else
            {
                SetStatus("Added " + name);
            }

            PropsListBox.Items.Add(name);
        }

        private void UpdateModeControls()
        {
            bool grass = IsGrassMode;
            PropsGroupBox.Visible = !grass;
            GrassGroupBox.Visible = grass;
            RandomYawCheckBox.Enabled = !grass;
            AlignToSurfaceCheckBox.Enabled = !grass;
            ScaleMinNumericUpDown.Enabled = !grass;
            ScaleMaxNumericUpDown.Enabled = !grass;
            ScaleLabel.Enabled = !grass;
        }

        private void UpdateTargetLabel()
        {
            UpdateTargetLabel(ProjectForm?.GetPaintPropsTargetYmap());
        }
        private void UpdateTargetLabel(YmapFile ymap)
        {
            TargetYmapTextBox.Text = (ymap != null) ? ymap.Name : "(select a project ymap)";
        }

        private void SetStatus(string text)
        {
            if (InvokeRequired)
            {
                try { BeginInvoke(new Action(() => { StatusLabel.Text = text; })); } catch { }
                return;
            }
            StatusLabel.Text = text;
        }


        private void AddButton_Click(object sender, EventArgs e)
        {
            AddProp(PropNameTextBox.Text);
            PropNameTextBox.Text = string.Empty;
        }

        private void PropNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                AddButton_Click(sender, e);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void AddSelectedButton_Click(object sender, EventArgs e)
        {
            var name = ProjectForm?.GetSelectedEntityArchetypeName();
            if (string.IsNullOrEmpty(name))
            {
                SetStatus("Select an entity first (in the world view or the project tree).");
                return;
            }
            AddProp(name);
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            var idx = PropsListBox.SelectedIndex;
            if (idx < 0) return;
            PropsListBox.Items.RemoveAt(idx);
            if (PropsListBox.Items.Count > 0)
            {
                PropsListBox.SelectedIndex = Math.Min(idx, PropsListBox.Items.Count - 1);
            }
        }

        private void PaintEnabledCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTargetLabel();
            if (PaintEnabledCheckBox.Checked && !IsGrassMode && (PropsListBox.Items.Count == 0))
            {
                SetStatus("Add at least one prop to the list to start painting.");
            }
        }

        private void ModeRadio_CheckedChanged(object sender, EventArgs e)
        {
            UpdateModeControls();
        }

        private void GrassColorLabel_Click(object sender, EventArgs e)
        {
            var colDiag = new ColourPickerForm { SelectedColour = GrassColorLabel.BackColor };
            if (colDiag.ShowDialog(this) == DialogResult.OK)
            {
                GrassColorLabel.BackColor = colDiag.SelectedColour;
            }
        }

        private void ScaleMinNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (ScaleMaxNumericUpDown.Value < ScaleMinNumericUpDown.Value)
            {
                ScaleMaxNumericUpDown.Value = ScaleMinNumericUpDown.Value;
            }
        }

        private void ScaleMaxNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (ScaleMinNumericUpDown.Value > ScaleMaxNumericUpDown.Value)
            {
                ScaleMinNumericUpDown.Value = ScaleMaxNumericUpDown.Value;
            }
        }

        private void GrassScaleMinNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (GrassScaleMaxNumericUpDown.Value < GrassScaleMinNumericUpDown.Value)
            {
                GrassScaleMaxNumericUpDown.Value = GrassScaleMinNumericUpDown.Value;
            }
        }

        private void GrassScaleMaxNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (GrassScaleMinNumericUpDown.Value > GrassScaleMaxNumericUpDown.Value)
            {
                GrassScaleMinNumericUpDown.Value = GrassScaleMaxNumericUpDown.Value;
            }
        }
    }
}

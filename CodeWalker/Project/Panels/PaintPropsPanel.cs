using CodeWalker.GameFiles;
using CodeWalker.World;
using SharpDX;
using System;
using System.Collections.Generic;
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

        internal bool PaintEnabled => PaintEnabledCheckBox.Checked && (PropsListBox.Items.Count > 0);
        internal float BrushRadius => (float)RadiusNumericUpDown.Value;


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
                var hashes = new HashSet<uint>();
                foreach (var item in PropsListBox.Items)
                {
                    hashes.Add(JenkHash.GenHash(item.ToString().ToLowerInvariant()));
                }
                int removed = ProjectForm.ErasePaintedEntities(ymap, mouseRay.Position, BrushRadius, hashes);
                if (removed > 0)
                {
                    SetStatus("Erased " + removed.ToString() + " prop(s) from " + ymap.Name);
                }
                return;
            }

            var archetypes = GetPaletteArchetypes();
            if (archetypes.Count == 0)
            {
                SetStatus("None of the props in the list were found in the game files.");
                return;
            }

            float radius = BrushRadius;
            int count = (int)CountNumericUpDown.Value;
            float spacing = (float)SpacingNumericUpDown.Value;
            float minSlopeCos = (float)Math.Cos(MathUtil.DegreesToRadians((float)MaxSlopeNumericUpDown.Value));
            float scaleMin = (float)ScaleMinNumericUpDown.Value;
            float scaleMax = Math.Max(scaleMin, (float)ScaleMaxNumericUpDown.Value);
            float zOffset = (float)ZOffsetNumericUpDown.Value;
            bool randomYaw = RandomYawCheckBox.Checked;
            bool alignToSurface = AlignToSurfaceCheckBox.Checked;

            var placed = new List<Vector3>();
            int created = 0;
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
                if (IsTooClose(pos, spacing, ymap, placed)) continue;

                var arch = archetypes[Rand.Next(archetypes.Count)];
                float yaw = randomYaw ? (float)(Rand.NextDouble() * Math.PI * 2.0) : 0.0f;
                var ori = GetOrientation(alignToSurface ? normal : Vector3.UnitZ, yaw);
                float scale = scaleMin + (float)Rand.NextDouble() * (scaleMax - scaleMin);

                var ent = ProjectForm.PaintEntity(ymap, arch, pos, ori, scale);
                if (ent != null)
                {
                    placed.Add(pos);
                    created++;
                }
            }

            if (created > 0)
            {
                SetStatus("Painted " + created.ToString() + " prop(s) into " + ymap.Name + " (" + (ymap.AllEntities?.Length ?? 0).ToString() + " entities)");
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

        private bool IsTooClose(Vector3 pos, float spacing, YmapFile ymap, List<Vector3> placed)
        {
            if (spacing <= 0) return false;
            float sq = spacing * spacing;
            foreach (var p in placed)
            {
                if (Vector3.DistanceSquared(p, pos) < sq) return true;
            }
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
            if (PaintEnabledCheckBox.Checked && (PropsListBox.Items.Count == 0))
            {
                SetStatus("Add at least one prop to the list to start painting.");
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
    }
}

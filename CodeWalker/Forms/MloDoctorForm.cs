using CodeWalker.GameFiles;
using CodeWalker.Tools;
using Quaternion = SharpDX.Quaternion;
using Vector3 = SharpDX.Vector3;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CodeWalker.Forms
{
    //MLO Doctor: scans a placed interior for problems, shows them in the 3D world, previews and applies safe fixes,
    //and simulates which rooms can be seen from a position through the portal graph.
    public class MloDoctorForm : Form
    {
        private readonly WorldForm wf;
        public YmapEntityDef Instance { get; private set; } //the placed interior, used to put local coordinates into the world
        public MloArchetype Mlo { get; private set; }
        public List<MloIssue> Issues { get; private set; } = new List<MloIssue>();
        public MloIssue SelectedIssue { get; private set; }
        public bool ShowAll { get { return showAllCheck.Checked; } }
        public MloVisibilityResult Visibility { get; private set; }
        public Vector3 Eye { get; private set; } //local space
        public bool HasEye { get; private set; }

        private Label headerLabel;
        private ListView list;
        private TextBox detail;
        private Button goButton, fixButton, fixAllButton;
        private CheckBox showAllCheck;
        private ComboBox startCombo;
        private NumericUpDown depthBox;
        private TextBox visText;

        public MloDoctorForm(WorldForm worldForm, YmapEntityDef instance)
        {
            wf = worldForm;
            Text = "MLO Doctor";
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            ClientSize = new Size(680, 760);
            MinimumSize = new Size(560, 600);
            Location = new Point(worldForm.Left + 30, worldForm.Top + 70);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Padding = new Padding(6) };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 105));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            Controls.Add(root);

            headerLabel = new Label { AutoSize = true, MaximumSize = new Size(650, 0) };
            root.Controls.Add(headerLabel, 0, 0);

            var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
            buttons.Controls.Add(Btn("Re-scan", (s, e) => Rescan()));
            goButton = Btn("Go to", (s, e) => GoToSelected());
            buttons.Controls.Add(goButton);
            fixButton = Btn("Apply this fix", (s, e) => ApplyFix(SelectedIssue));
            buttons.Controls.Add(fixButton);
            fixAllButton = Btn("Apply all safe fixes", (s, e) => ApplyAll());
            buttons.Controls.Add(fixAllButton);
            showAllCheck = new CheckBox { Text = "Show all problems in 3D", AutoSize = true, Checked = true, Padding = new Padding(8, 4, 0, 0) };
            buttons.Controls.Add(showAllCheck);
            root.Controls.Add(buttons, 0, 1);

            list = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false, MultiSelect = false, GridLines = true };
            list.Columns.Add("", 60);
            list.Columns.Add("Check", 90);
            list.Columns.Add("Problem", 480);
            list.SelectedIndexChanged += (s, e) => OnSelectionChanged();
            list.DoubleClick += (s, e) => GoToSelected();
            root.Controls.Add(list, 0, 2);

            detail = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical };
            root.Controls.Add(detail, 0, 3);

            var visGroup = new GroupBox { Text = "Visibility simulation (which rooms can be seen from a point)", Dock = DockStyle.Fill, AutoSize = true };
            var vf = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            vf.Controls.Add(new Label { Text = "Eye is in:", AutoSize = true, Padding = new Padding(0, 6, 0, 0) });
            startCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
            vf.Controls.Add(startCombo);
            vf.Controls.Add(new Label { Text = "Max portals deep:", AutoSize = true, Padding = new Padding(6, 6, 0, 0) });
            depthBox = new NumericUpDown { Minimum = 1, Maximum = 20, Value = 8, Width = 50 };
            vf.Controls.Add(depthBox);
            vf.Controls.Add(Btn("Simulate from camera position", (s, e) => Simulate(true), 190));
            vf.Controls.Add(Btn("Simulate from room centre", (s, e) => Simulate(false), 170));
            vf.Controls.Add(Btn("Clear", (s, e) => { Visibility = null; HasEye = false; visText.Text = ""; }, 60));
            visGroup.Controls.Add(vf);
            root.Controls.Add(visGroup, 0, 4);

            visText = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical };
            root.Controls.Add(visText, 0, 5);

            SetInstance(instance);
        }

        private Button Btn(string text, EventHandler click, int width = 120)
        {
            var b = new Button { Text = text, Width = width, Height = 26 };
            b.Click += click;
            return b;
        }

        public void SetInstance(YmapEntityDef instance)
        {
            Instance = instance;
            Mlo = instance?.Archetype as MloArchetype;
            Visibility = null;
            HasEye = false;
            visText.Text = "";
            startCombo.Items.Clear();
            if (Mlo?.rooms != null)
            {
                startCombo.Items.Add("(room under the camera)");
                for (int i = 1; i < Mlo.rooms.Length; i++) startCombo.Items.Add(i + ": " + Mlo.rooms[i].RoomName);
                startCombo.SelectedIndex = 0;
            }
            Rescan();
        }

        private bool CanEdit()
        {
            var ytyp = Mlo?.Ytyp;
            var pf = wf.ProjectFormInstance;
            return (ytyp != null) && (pf != null) && pf.YtypExistsInProject(ytyp);
        }

        public void Rescan()
        {
            SelectedIssue = null;
            if (Mlo == null)
            {
                headerLabel.Text = "No interior selected. Select an MLO (or something inside it) in the world and open the doctor again.";
                Issues = new List<MloIssue>();
                list.Items.Clear();
                return;
            }
            Issues = MloDoctor.Analyze(Mlo, wf.GameFileCache);
            int err = Issues.Count(i => i.Severity == MloIssueSeverity.Error);
            int warn = Issues.Count(i => i.Severity == MloIssueSeverity.Warning);
            headerLabel.Text = Mlo.Name + ": " + (Mlo.rooms?.Length ?? 0) + " rooms, " + (Mlo.portals?.Length ?? 0) + " portals, " + (Mlo.entities?.Length ?? 0) + " entities, " + (Mlo.entitySets?.Length ?? 0) + " entity sets.\r\n" +
                err + " errors, " + warn + " warnings, " + (Issues.Count - err - warn) + " notes. " +
                (CanEdit() ? "Fixes will edit this interior's ytyp in your project." : "Fixes are disabled: add this interior's ytyp to your project first (a game file can't be edited).");

            list.BeginUpdate();
            list.Items.Clear();
            foreach (var iss in Issues)
            {
                var item = new ListViewItem(iss.Severity.ToString());
                item.SubItems.Add(iss.Check);
                item.SubItems.Add(iss.Message + (iss.Fixable ? "   [fix available]" : ""));
                item.Tag = iss;
                item.ForeColor = iss.Severity == MloIssueSeverity.Error ? Color.Firebrick : iss.Severity == MloIssueSeverity.Warning ? Color.DarkOrange : Color.DimGray;
                list.Items.Add(item);
            }
            list.EndUpdate();
            if (Issues.Count == 0) detail.Text = "No problems found. That doesn't prove the interior is perfect - the doctor checks structure, not engine-specific behaviour. Confirm in game.";
            else detail.Text = "Click a problem to see it highlighted in the 3D view.";
            UpdateButtons();
        }

        private void OnSelectionChanged()
        {
            SelectedIssue = (list.SelectedItems.Count > 0) ? list.SelectedItems[0].Tag as MloIssue : null;
            var sb = new StringBuilder();
            if (SelectedIssue != null)
            {
                sb.AppendLine(SelectedIssue.Message);
                sb.AppendLine();
                sb.AppendLine("Likely cause: " + SelectedIssue.Cause);
                if (SelectedIssue.Fixable)
                {
                    sb.AppendLine();
                    sb.AppendLine("Safe fix: " + SelectedIssue.FixDescription + ".");
                    sb.AppendLine("The green outline in the 3D view shows the result. Nothing changes until you press Apply.");
                }
                else sb.AppendLine("\r\nNo automatic fix - this one needs a manual change.");
            }
            detail.Text = sb.ToString();
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            bool edit = CanEdit();
            goButton.Enabled = (SelectedIssue != null) && SelectedIssue.HasFocus;
            fixButton.Enabled = edit && (SelectedIssue != null) && SelectedIssue.Fixable;
            fixAllButton.Enabled = edit && Issues.Any(i => i.Fixable);
        }

        public Vector3 ToWorld(Vector3 local)
        {
            return Instance.Position + Instance.Orientation.Multiply(local);
        }
        public Vector3 ToLocal(Vector3 world)
        {
            return Quaternion.Invert(Instance.Orientation).Multiply(world - Instance.Position);
        }

        private void GoToSelected()
        {
            if ((SelectedIssue == null) || !SelectedIssue.HasFocus || (Instance == null)) return;
            wf.GoToPosition(ToWorld(SelectedIssue.Focus), new Vector3(6, 6, 6));
        }

        private bool ApplyFixCore(MloIssue iss)
        {
            try
            {
                lock (wf.RenderSyncRoot)
                {
                    iss.Fix();
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("That fix failed and may be incomplete: " + ex.Message);
                return false;
            }
        }
        private void AfterFixes()
        {
            Mlo.UpdatePortalCounts();
            Instance?.MloInstance?.UpdateBBs(Mlo);
            wf.NotifyMloEdited(Mlo);
            Rescan();
        }
        private void ApplyFix(MloIssue iss)
        {
            if ((iss == null) || !iss.Fixable || !CanEdit()) return;
            if (ApplyFixCore(iss)) AfterFixes();
        }
        private void ApplyAll()
        {
            if (!CanEdit()) return;
            var fixes = Issues.Where(i => i.Fixable).ToList();
            if (fixes.Count == 0) return;
            if (MessageBox.Show("Apply " + fixes.Count + " fixes to " + Mlo.Name + "?\r\n\r\nEach one is listed with a preview; applying all at once skips the preview. There is no undo for these, so save a copy of your ytyp first.", "MLO Doctor", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            int done = 0;
            foreach (var f in fixes) if (ApplyFixCore(f)) done++;
            AfterFixes();
            MessageBox.Show(done + " fixes applied. Review the re-scan results and save your ytyp.");
        }

        private void Simulate(bool fromCamera)
        {
            if ((Mlo?.rooms == null) || (Instance == null)) return;
            int room;
            Vector3 eye;
            int sel = startCombo.SelectedIndex;
            if (fromCamera)
            {
                eye = ToLocal(wf.CameraPosition);
                room = (sel > 0) ? sel : MloDoctor.FindRoom(Mlo, eye);
                if (room < 0)
                {
                    visText.Text = "The camera isn't inside any room's bounds. Fly into the interior, or pick a room from the list and use 'Simulate from room centre'.";
                    return;
                }
            }
            else
            {
                room = (sel > 0) ? sel : MloDoctor.FindRoom(Mlo, ToLocal(wf.CameraPosition));
                if (room < 1)
                {
                    visText.Text = "Pick a room from the list first.";
                    return;
                }
                eye = Mlo.rooms[room].BBCenter;
            }
            var res = MloDoctor.SimulateVisibility(Mlo, eye, room, (int)depthBox.Value);
            Eye = eye;
            HasEye = true;
            Visibility = res;

            var sb = new StringBuilder();
            sb.AppendLine("Looking from " + (fromCamera ? "the camera" : "the centre") + " of " + RoomName(room) + " (approximate; confirm engine behaviour in game).");
            sb.AppendLine();
            var seen = res.Visible.Where(r => r != room).OrderBy(r => res.Hops.ContainsKey(r) ? res.Hops[r] : 99).ToList();
            sb.AppendLine("Can be seen through portals (" + seen.Count + "):");
            foreach (var r in seen) sb.AppendLine("   " + RoomName(r) + "  - " + (res.Hops.ContainsKey(r) ? res.Hops[r] + " portal(s) away" : "?"));
            var hidden = res.Hops.Keys.Where(r => (r != room) && !res.Visible.Contains(r)).OrderBy(r => res.Hops[r]).ToList();
            sb.AppendLine();
            sb.AppendLine("Connected but NOT visible from here (" + hidden.Count + "):");
            foreach (var r in hidden) sb.AppendLine("   " + RoomName(r) + "  - " + res.Hops[r] + " portal(s) away");
            var cut = Enumerable.Range(1, Mlo.rooms.Length - 1).Where(r => !res.Hops.ContainsKey(r)).ToList();
            if (cut.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Not connected to this room at all (" + cut.Count + "): " + string.Join(", ", cut.Select(RoomName)));
            }
            sb.AppendLine();
            sb.AppendLine("The outside world is " + (res.OutsideVisible ? "visible from here." : "not visible from here."));
            if (seen.Count > 6) sb.AppendLine("Note: " + seen.Count + " other rooms are visible from this spot - that is a lot to draw at once.");
            if (res.Truncated) sb.AppendLine("The search hit its limit, so very complex portal layouts may be under-reported.");
            sb.AppendLine("Green boxes in 3D = visible, orange = connected but hidden, cyan = start.");
            visText.Text = sb.ToString();
        }

        private string RoomName(int i)
        {
            var n = (Mlo?.rooms != null && i >= 0 && i < Mlo.rooms.Length) ? Mlo.rooms[i].RoomName : "";
            return i + (string.IsNullOrEmpty(n) ? "" : " '" + n + "'");
        }
    }
}

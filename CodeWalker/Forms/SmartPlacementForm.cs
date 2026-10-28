using CodeWalker.World;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CodeWalker.Forms
{
    //Smart placement & alignment tool window. Every command only changes a preview (shown as ghost boxes in
    //the world); nothing is moved until Apply is pressed, and Apply can be undone with the normal undo.
    public class SmartPlacementForm : Form
    {
        private readonly WorldForm wf;
        private Label infoLabel;
        private CheckBox toAnchorCheck;
        private CheckBox equalGapsCheck;
        private CheckBox flipCheck;
        private CheckBox tiltCheck;
        private CheckBox wallAlignCheck;
        private CheckBox lockElevCheck;
        private NumericUpDown wallDistBox;

        public SmartPlacementForm(WorldForm worldForm)
        {
            wf = worldForm;
            Text = "Smart placement && alignment";
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            ClientSize = new Size(360, 620);
            Location = new Point(worldForm.Left + 40, worldForm.Top + 80);

            var root = new FlowLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.FlowDirection = FlowDirection.TopDown;
            root.WrapContents = false;
            root.AutoScroll = true;
            root.Padding = new Padding(6);
            Controls.Add(root);

            infoLabel = new Label();
            infoLabel.AutoSize = false;
            infoLabel.Size = new Size(335, 48);
            infoLabel.Text = "Select two or more entities (Ctrl+click), then use the buttons below. The last one you click is the anchor.";
            root.Controls.Add(infoLabel);

            var reload = MakeButton("Reload selection", (s, e) => { wf.BeginSmartPlacement(); UpdateInfo(); });
            root.Controls.Add(reload);

            var align = Group(root, "Align", 3);
            toAnchorCheck = Check(align, "Align to the anchor (last selected) instead of the group", false);
            string[] axes = { "X", "Y", "Z" };
            string[][] names = { new[] { "Left", "Center", "Right" }, new[] { "Front", "Center", "Back" }, new[] { "Bottom", "Center", "Top" } };
            for (int ax = 0; ax < 3; ax++)
            {
                for (int m = 0; m < 3; m++)
                {
                    int axis = ax;
                    var mode = (PlacementAlign)m;
                    align.Controls.Add(MakeButton(axes[ax] + " " + names[ax][m], (s, e) => Run(sp => sp.Align(axis, mode, toAnchorCheck.Checked)), 104));
                }
            }

            var dist = Group(root, "Equal spacing", 3);
            equalGapsCheck = Check(dist, "Equal gaps between models (off = equal centres)", true);
            for (int ax = 0; ax < 3; ax++)
            {
                int axis = ax;
                dist.Controls.Add(MakeButton("Along " + axes[ax], (s, e) => Run(sp =>
                {
                    if (!sp.Distribute(axis, equalGapsCheck.Checked)) Info("Equal spacing needs at least 3 entities.");
                }), 104));
            }

            var rot = Group(root, "Rotation", 3);
            flipCheck = Check(rot, "Flip (for models that face the other way)", false);
            rot.Controls.Add(MakeButton("Match rotation", (s, e) => Run(sp => sp.MatchRotation(false)), 104));
            rot.Controls.Add(MakeButton("Match heading", (s, e) => Run(sp => sp.MatchRotation(true)), 104));
            rot.Controls.Add(MakeButton("Face anchor", (s, e) => Run(sp => sp.FaceAnchor(flipCheck.Checked)), 104));

            var surf = Group(root, "Surfaces (uses collision and model bounds)", 2);
            tiltCheck = Check(surf, "Tilt to follow the slope", false);
            wallAlignCheck = Check(surf, "Turn to face away from the wall", true);
            var distPanel = new FlowLayoutPanel();
            distPanel.AutoSize = true;
            distPanel.Controls.Add(new Label { Text = "Wall search distance (m):", AutoSize = true, Padding = new Padding(0, 6, 0, 0) });
            wallDistBox = new NumericUpDown { Minimum = 0.1m, Maximum = 50, DecimalPlaces = 1, Value = 3, Width = 60 };
            distPanel.Controls.Add(wallDistBox);
            surf.Controls.Add(distPanel);
            surf.SetFlowBreak(distPanel, true);
            surf.Controls.Add(MakeButton("Fit to surface", (s, e) => Run(sp =>
            {
                int n = sp.FitToSurface(wf.Space, tiltCheck.Checked, 45.0f);
                if (n == 0) Info("Nothing found underneath. Make sure collision is visible/loaded nearby.");
            }), 160));
            surf.Controls.Add(MakeButton("Snap to wall", (s, e) => Run(sp =>
            {
                int n = sp.SnapToWall(wf.Space, (float)wallDistBox.Value, wallAlignCheck.Checked, flipCheck.Checked);
                if (n == 0) Info("No wall found within that distance.");
            }), 160));

            var move = Group(root, "Moving", 1);
            lockElevCheck = Check(move, "Move without changing elevation (dragging only moves sideways)", false);
            lockElevCheck.CheckedChanged += (s, e) => { wf.LockElevation = lockElevCheck.Checked; };

            var final = new FlowLayoutPanel();
            final.AutoSize = true;
            final.Margin = new Padding(0, 8, 0, 0);
            final.Controls.Add(MakeButton("Apply", (s, e) => { int n = wf.CommitSmartPlacement(); UpdateInfo(); Info(n + " entities moved. Ctrl+Z undoes it."); }, 105));
            final.Controls.Add(MakeButton("Reset preview", (s, e) => { Run(sp => sp.Reset()); }, 105));
            final.Controls.Add(MakeButton("Close", (s, e) => Close(), 105));
            root.Controls.Add(final);

            Activated += (s, e) =>
            {
                //pick up a changed selection, unless there is an unapplied preview in progress
                var sp = wf.SmartPlacementSession;
                if ((sp == null) || !sp.Changed) wf.BeginSmartPlacement();
                UpdateInfo();
            };
        }

        private void Run(Action<SmartPlacement> op)
        {
            var sp = wf.SmartPlacementSession;
            if (sp == null)
            {
                Info("Select some entities in the world first, then click Reload selection.");
                return;
            }
            lock (wf.RenderSyncRoot)
            {
                op(sp);
            }
            UpdateInfo();
        }

        private void Info(string text)
        {
            infoLabel.Text = text;
        }

        public void UpdateInfo()
        {
            var sp = wf.SmartPlacementSession;
            if (sp == null)
            {
                infoLabel.Text = "No entities selected. Select two or more entities (Ctrl+click), then use the buttons below.";
                return;
            }
            int changed = sp.Items.Count(i => i.Changed);
            string anchor = (sp.Anchor != null) ? sp.Anchor.Entity.Archetype?.Name ?? sp.Anchor.Entity._CEntityDef.archetypeName.ToString() : "-";
            infoLabel.Text = sp.Items.Count + " selected, anchor: " + anchor + ".\r\n" +
                (changed > 0 ? changed + " will move - check the ghost boxes in the world, then Apply." : "Pick a command to preview it.");
        }

        private Button MakeButton(string text, EventHandler click, int width = 335)
        {
            var b = new Button();
            b.Text = text;
            b.Width = width;
            b.Height = 26;
            b.Click += click;
            return b;
        }

        private FlowLayoutPanel Group(FlowLayoutPanel parent, string title, int columns)
        {
            var box = new GroupBox();
            box.Text = title;
            box.Width = 335;
            box.AutoSize = true;
            box.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            var flow = new FlowLayoutPanel();
            flow.Dock = DockStyle.Fill;
            flow.AutoSize = true;
            flow.Width = 320;
            flow.WrapContents = true;
            box.Controls.Add(flow);
            parent.Controls.Add(box);
            return flow;
        }

        private CheckBox Check(FlowLayoutPanel parent, string text, bool value)
        {
            var c = new CheckBox();
            c.Text = text;
            c.Checked = value;
            c.AutoSize = false;
            c.Size = new Size(315, 22);
            parent.Controls.Add(c);
            parent.SetFlowBreak(c, true);
            return c;
        }
    }
}

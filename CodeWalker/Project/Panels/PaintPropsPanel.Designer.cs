namespace CodeWalker.Project.Panels
{
    partial class PaintPropsPanel
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.PaintEnabledCheckBox = new System.Windows.Forms.CheckBox();
            this.PropsGroupBox = new System.Windows.Forms.GroupBox();
            this.RemoveButton = new System.Windows.Forms.Button();
            this.PropsListBox = new System.Windows.Forms.ListBox();
            this.AddSelectedButton = new System.Windows.Forms.Button();
            this.AddButton = new System.Windows.Forms.Button();
            this.PropNameTextBox = new System.Windows.Forms.TextBox();
            this.BrushGroupBox = new System.Windows.Forms.GroupBox();
            this.SpacingNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.SpacingLabel = new System.Windows.Forms.Label();
            this.CountNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.CountLabel = new System.Windows.Forms.Label();
            this.RadiusNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.RadiusLabel = new System.Windows.Forms.Label();
            this.PlacementGroupBox = new System.Windows.Forms.GroupBox();
            this.ZOffsetNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.ZOffsetLabel = new System.Windows.Forms.Label();
            this.ScaleMaxNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.ScaleMinNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.ScaleLabel = new System.Windows.Forms.Label();
            this.MaxSlopeNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.MaxSlopeLabel = new System.Windows.Forms.Label();
            this.AlignToSurfaceCheckBox = new System.Windows.Forms.CheckBox();
            this.RandomYawCheckBox = new System.Windows.Forms.CheckBox();
            this.TargetYmapLabel = new System.Windows.Forms.Label();
            this.TargetYmapTextBox = new System.Windows.Forms.TextBox();
            this.HelpLabel = new System.Windows.Forms.Label();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.PropsGroupBox.SuspendLayout();
            this.BrushGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SpacingNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CountNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RadiusNumericUpDown)).BeginInit();
            this.PlacementGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ZOffsetNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScaleMaxNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScaleMinNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxSlopeNumericUpDown)).BeginInit();
            this.SuspendLayout();
            //
            // PaintEnabledCheckBox
            //
            this.PaintEnabledCheckBox.AutoSize = true;
            this.PaintEnabledCheckBox.Location = new System.Drawing.Point(12, 12);
            this.PaintEnabledCheckBox.Name = "PaintEnabledCheckBox";
            this.PaintEnabledCheckBox.Size = new System.Drawing.Size(300, 17);
            this.PaintEnabledCheckBox.TabIndex = 0;
            this.PaintEnabledCheckBox.Text = "&Paint enabled (hold CTRL and left-drag in the world view)";
            this.PaintEnabledCheckBox.UseVisualStyleBackColor = true;
            this.PaintEnabledCheckBox.CheckedChanged += new System.EventHandler(this.PaintEnabledCheckBox_CheckedChanged);
            //
            // PropsGroupBox
            //
            this.PropsGroupBox.Controls.Add(this.RemoveButton);
            this.PropsGroupBox.Controls.Add(this.PropsListBox);
            this.PropsGroupBox.Controls.Add(this.AddSelectedButton);
            this.PropsGroupBox.Controls.Add(this.AddButton);
            this.PropsGroupBox.Controls.Add(this.PropNameTextBox);
            this.PropsGroupBox.Location = new System.Drawing.Point(12, 38);
            this.PropsGroupBox.Name = "PropsGroupBox";
            this.PropsGroupBox.Size = new System.Drawing.Size(406, 170);
            this.PropsGroupBox.TabIndex = 1;
            this.PropsGroupBox.TabStop = false;
            this.PropsGroupBox.Text = "Props (one is picked at random for each placement)";
            //
            // PropNameTextBox
            //
            this.PropNameTextBox.Location = new System.Drawing.Point(8, 20);
            this.PropNameTextBox.Name = "PropNameTextBox";
            this.PropNameTextBox.Size = new System.Drawing.Size(250, 20);
            this.PropNameTextBox.TabIndex = 0;
            this.PropNameTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PropNameTextBox_KeyDown);
            //
            // AddButton
            //
            this.AddButton.Location = new System.Drawing.Point(264, 18);
            this.AddButton.Name = "AddButton";
            this.AddButton.Size = new System.Drawing.Size(60, 23);
            this.AddButton.TabIndex = 1;
            this.AddButton.Text = "Add";
            this.AddButton.UseVisualStyleBackColor = true;
            this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
            //
            // AddSelectedButton
            //
            this.AddSelectedButton.Location = new System.Drawing.Point(330, 18);
            this.AddSelectedButton.Name = "AddSelectedButton";
            this.AddSelectedButton.Size = new System.Drawing.Size(68, 23);
            this.AddSelectedButton.TabIndex = 2;
            this.AddSelectedButton.Text = "Selected";
            this.AddSelectedButton.UseVisualStyleBackColor = true;
            this.AddSelectedButton.Click += new System.EventHandler(this.AddSelectedButton_Click);
            //
            // PropsListBox
            //
            this.PropsListBox.FormattingEnabled = true;
            this.PropsListBox.Location = new System.Drawing.Point(8, 47);
            this.PropsListBox.Name = "PropsListBox";
            this.PropsListBox.Size = new System.Drawing.Size(316, 108);
            this.PropsListBox.TabIndex = 3;
            //
            // RemoveButton
            //
            this.RemoveButton.Location = new System.Drawing.Point(330, 47);
            this.RemoveButton.Name = "RemoveButton";
            this.RemoveButton.Size = new System.Drawing.Size(68, 23);
            this.RemoveButton.TabIndex = 4;
            this.RemoveButton.Text = "Remove";
            this.RemoveButton.UseVisualStyleBackColor = true;
            this.RemoveButton.Click += new System.EventHandler(this.RemoveButton_Click);
            //
            // BrushGroupBox
            //
            this.BrushGroupBox.Controls.Add(this.SpacingNumericUpDown);
            this.BrushGroupBox.Controls.Add(this.SpacingLabel);
            this.BrushGroupBox.Controls.Add(this.CountNumericUpDown);
            this.BrushGroupBox.Controls.Add(this.CountLabel);
            this.BrushGroupBox.Controls.Add(this.RadiusNumericUpDown);
            this.BrushGroupBox.Controls.Add(this.RadiusLabel);
            this.BrushGroupBox.Location = new System.Drawing.Point(12, 214);
            this.BrushGroupBox.Name = "BrushGroupBox";
            this.BrushGroupBox.Size = new System.Drawing.Size(200, 150);
            this.BrushGroupBox.TabIndex = 2;
            this.BrushGroupBox.TabStop = false;
            this.BrushGroupBox.Text = "Brush";
            //
            // RadiusLabel
            //
            this.RadiusLabel.AutoSize = true;
            this.RadiusLabel.Location = new System.Drawing.Point(6, 24);
            this.RadiusLabel.Name = "RadiusLabel";
            this.RadiusLabel.Size = new System.Drawing.Size(40, 13);
            this.RadiusLabel.TabIndex = 0;
            this.RadiusLabel.Text = "Radius";
            //
            // RadiusNumericUpDown
            //
            this.RadiusNumericUpDown.DecimalPlaces = 1;
            this.RadiusNumericUpDown.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.RadiusNumericUpDown.Location = new System.Drawing.Point(110, 22);
            this.RadiusNumericUpDown.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.RadiusNumericUpDown.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.RadiusNumericUpDown.Name = "RadiusNumericUpDown";
            this.RadiusNumericUpDown.Size = new System.Drawing.Size(80, 20);
            this.RadiusNumericUpDown.TabIndex = 1;
            this.RadiusNumericUpDown.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            //
            // CountLabel
            //
            this.CountLabel.AutoSize = true;
            this.CountLabel.Location = new System.Drawing.Point(6, 50);
            this.CountLabel.Name = "CountLabel";
            this.CountLabel.Size = new System.Drawing.Size(76, 13);
            this.CountLabel.TabIndex = 2;
            this.CountLabel.Text = "Props per dab";
            //
            // CountNumericUpDown
            //
            this.CountNumericUpDown.Location = new System.Drawing.Point(110, 48);
            this.CountNumericUpDown.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.CountNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.CountNumericUpDown.Name = "CountNumericUpDown";
            this.CountNumericUpDown.Size = new System.Drawing.Size(80, 20);
            this.CountNumericUpDown.TabIndex = 3;
            this.CountNumericUpDown.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            //
            // SpacingLabel
            //
            this.SpacingLabel.AutoSize = true;
            this.SpacingLabel.Location = new System.Drawing.Point(6, 76);
            this.SpacingLabel.Name = "SpacingLabel";
            this.SpacingLabel.Size = new System.Drawing.Size(66, 13);
            this.SpacingLabel.TabIndex = 4;
            this.SpacingLabel.Text = "Min spacing";
            //
            // SpacingNumericUpDown
            //
            this.SpacingNumericUpDown.DecimalPlaces = 1;
            this.SpacingNumericUpDown.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.SpacingNumericUpDown.Location = new System.Drawing.Point(110, 74);
            this.SpacingNumericUpDown.Name = "SpacingNumericUpDown";
            this.SpacingNumericUpDown.Size = new System.Drawing.Size(80, 20);
            this.SpacingNumericUpDown.TabIndex = 5;
            this.SpacingNumericUpDown.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            //
            // PlacementGroupBox
            //
            this.PlacementGroupBox.Controls.Add(this.ZOffsetNumericUpDown);
            this.PlacementGroupBox.Controls.Add(this.ZOffsetLabel);
            this.PlacementGroupBox.Controls.Add(this.ScaleMaxNumericUpDown);
            this.PlacementGroupBox.Controls.Add(this.ScaleMinNumericUpDown);
            this.PlacementGroupBox.Controls.Add(this.ScaleLabel);
            this.PlacementGroupBox.Controls.Add(this.MaxSlopeNumericUpDown);
            this.PlacementGroupBox.Controls.Add(this.MaxSlopeLabel);
            this.PlacementGroupBox.Controls.Add(this.AlignToSurfaceCheckBox);
            this.PlacementGroupBox.Controls.Add(this.RandomYawCheckBox);
            this.PlacementGroupBox.Location = new System.Drawing.Point(218, 214);
            this.PlacementGroupBox.Name = "PlacementGroupBox";
            this.PlacementGroupBox.Size = new System.Drawing.Size(200, 150);
            this.PlacementGroupBox.TabIndex = 3;
            this.PlacementGroupBox.TabStop = false;
            this.PlacementGroupBox.Text = "Placement";
            //
            // RandomYawCheckBox
            //
            this.RandomYawCheckBox.AutoSize = true;
            this.RandomYawCheckBox.Checked = true;
            this.RandomYawCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.RandomYawCheckBox.Location = new System.Drawing.Point(8, 20);
            this.RandomYawCheckBox.Name = "RandomYawCheckBox";
            this.RandomYawCheckBox.Size = new System.Drawing.Size(105, 17);
            this.RandomYawCheckBox.TabIndex = 0;
            this.RandomYawCheckBox.Text = "Random rotation";
            this.RandomYawCheckBox.UseVisualStyleBackColor = true;
            //
            // AlignToSurfaceCheckBox
            //
            this.AlignToSurfaceCheckBox.AutoSize = true;
            this.AlignToSurfaceCheckBox.Location = new System.Drawing.Point(8, 42);
            this.AlignToSurfaceCheckBox.Name = "AlignToSurfaceCheckBox";
            this.AlignToSurfaceCheckBox.Size = new System.Drawing.Size(150, 17);
            this.AlignToSurfaceCheckBox.TabIndex = 1;
            this.AlignToSurfaceCheckBox.Text = "Tilt to match the ground";
            this.AlignToSurfaceCheckBox.UseVisualStyleBackColor = true;
            //
            // MaxSlopeLabel
            //
            this.MaxSlopeLabel.AutoSize = true;
            this.MaxSlopeLabel.Location = new System.Drawing.Point(6, 68);
            this.MaxSlopeLabel.Name = "MaxSlopeLabel";
            this.MaxSlopeLabel.Size = new System.Drawing.Size(80, 13);
            this.MaxSlopeLabel.TabIndex = 2;
            this.MaxSlopeLabel.Text = "Max slope (deg)";
            //
            // MaxSlopeNumericUpDown
            //
            this.MaxSlopeNumericUpDown.Location = new System.Drawing.Point(110, 66);
            this.MaxSlopeNumericUpDown.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.MaxSlopeNumericUpDown.Name = "MaxSlopeNumericUpDown";
            this.MaxSlopeNumericUpDown.Size = new System.Drawing.Size(80, 20);
            this.MaxSlopeNumericUpDown.TabIndex = 3;
            this.MaxSlopeNumericUpDown.Value = new decimal(new int[] {
            45,
            0,
            0,
            0});
            //
            // ScaleLabel
            //
            this.ScaleLabel.AutoSize = true;
            this.ScaleLabel.Location = new System.Drawing.Point(6, 94);
            this.ScaleLabel.Name = "ScaleLabel";
            this.ScaleLabel.Size = new System.Drawing.Size(34, 13);
            this.ScaleLabel.TabIndex = 4;
            this.ScaleLabel.Text = "Scale";
            //
            // ScaleMinNumericUpDown
            //
            this.ScaleMinNumericUpDown.DecimalPlaces = 2;
            this.ScaleMinNumericUpDown.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.ScaleMinNumericUpDown.Location = new System.Drawing.Point(64, 92);
            this.ScaleMinNumericUpDown.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.ScaleMinNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.ScaleMinNumericUpDown.Name = "ScaleMinNumericUpDown";
            this.ScaleMinNumericUpDown.Size = new System.Drawing.Size(60, 20);
            this.ScaleMinNumericUpDown.TabIndex = 5;
            this.ScaleMinNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ScaleMinNumericUpDown.ValueChanged += new System.EventHandler(this.ScaleMinNumericUpDown_ValueChanged);
            //
            // ScaleMaxNumericUpDown
            //
            this.ScaleMaxNumericUpDown.DecimalPlaces = 2;
            this.ScaleMaxNumericUpDown.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.ScaleMaxNumericUpDown.Location = new System.Drawing.Point(130, 92);
            this.ScaleMaxNumericUpDown.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.ScaleMaxNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.ScaleMaxNumericUpDown.Name = "ScaleMaxNumericUpDown";
            this.ScaleMaxNumericUpDown.Size = new System.Drawing.Size(60, 20);
            this.ScaleMaxNumericUpDown.TabIndex = 6;
            this.ScaleMaxNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ScaleMaxNumericUpDown.ValueChanged += new System.EventHandler(this.ScaleMaxNumericUpDown_ValueChanged);
            //
            // ZOffsetLabel
            //
            this.ZOffsetLabel.AutoSize = true;
            this.ZOffsetLabel.Location = new System.Drawing.Point(6, 120);
            this.ZOffsetLabel.Name = "ZOffsetLabel";
            this.ZOffsetLabel.Size = new System.Drawing.Size(80, 13);
            this.ZOffsetLabel.TabIndex = 7;
            this.ZOffsetLabel.Text = "Height offset";
            //
            // ZOffsetNumericUpDown
            //
            this.ZOffsetNumericUpDown.DecimalPlaces = 2;
            this.ZOffsetNumericUpDown.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.ZOffsetNumericUpDown.Location = new System.Drawing.Point(110, 118);
            this.ZOffsetNumericUpDown.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.ZOffsetNumericUpDown.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.ZOffsetNumericUpDown.Name = "ZOffsetNumericUpDown";
            this.ZOffsetNumericUpDown.Size = new System.Drawing.Size(80, 20);
            this.ZOffsetNumericUpDown.TabIndex = 8;
            //
            // TargetYmapLabel
            //
            this.TargetYmapLabel.AutoSize = true;
            this.TargetYmapLabel.Location = new System.Drawing.Point(12, 375);
            this.TargetYmapLabel.Name = "TargetYmapLabel";
            this.TargetYmapLabel.Size = new System.Drawing.Size(80, 13);
            this.TargetYmapLabel.TabIndex = 4;
            this.TargetYmapLabel.Text = "Paint into YMAP";
            //
            // TargetYmapTextBox
            //
            this.TargetYmapTextBox.Location = new System.Drawing.Point(110, 372);
            this.TargetYmapTextBox.Name = "TargetYmapTextBox";
            this.TargetYmapTextBox.ReadOnly = true;
            this.TargetYmapTextBox.Size = new System.Drawing.Size(308, 20);
            this.TargetYmapTextBox.TabIndex = 5;
            this.TargetYmapTextBox.TabStop = false;
            //
            // HelpLabel
            //
            this.HelpLabel.Location = new System.Drawing.Point(12, 400);
            this.HelpLabel.Name = "HelpLabel";
            this.HelpLabel.Size = new System.Drawing.Size(406, 30);
            this.HelpLabel.TabIndex = 6;
            this.HelpLabel.Text = "CTRL + left-drag paints props into the selected project YMAP. CTRL + SHIFT + left-" +
    "drag erases props from the list within the brush.";
            //
            // StatusLabel
            //
            this.StatusLabel.AutoEllipsis = true;
            this.StatusLabel.Location = new System.Drawing.Point(12, 434);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(406, 18);
            this.StatusLabel.TabIndex = 7;
            //
            // PaintPropsPanel
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 460);
            this.Controls.Add(this.StatusLabel);
            this.Controls.Add(this.HelpLabel);
            this.Controls.Add(this.TargetYmapTextBox);
            this.Controls.Add(this.TargetYmapLabel);
            this.Controls.Add(this.PlacementGroupBox);
            this.Controls.Add(this.BrushGroupBox);
            this.Controls.Add(this.PropsGroupBox);
            this.Controls.Add(this.PaintEnabledCheckBox);
            this.Name = "PaintPropsPanel";
            this.Text = "Paint Props";
            this.PropsGroupBox.ResumeLayout(false);
            this.PropsGroupBox.PerformLayout();
            this.BrushGroupBox.ResumeLayout(false);
            this.BrushGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SpacingNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CountNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RadiusNumericUpDown)).EndInit();
            this.PlacementGroupBox.ResumeLayout(false);
            this.PlacementGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ZOffsetNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScaleMaxNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScaleMinNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxSlopeNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox PaintEnabledCheckBox;
        private System.Windows.Forms.GroupBox PropsGroupBox;
        private System.Windows.Forms.Button RemoveButton;
        private System.Windows.Forms.ListBox PropsListBox;
        private System.Windows.Forms.Button AddSelectedButton;
        private System.Windows.Forms.Button AddButton;
        private System.Windows.Forms.TextBox PropNameTextBox;
        private System.Windows.Forms.GroupBox BrushGroupBox;
        private System.Windows.Forms.NumericUpDown SpacingNumericUpDown;
        private System.Windows.Forms.Label SpacingLabel;
        private System.Windows.Forms.NumericUpDown CountNumericUpDown;
        private System.Windows.Forms.Label CountLabel;
        private System.Windows.Forms.NumericUpDown RadiusNumericUpDown;
        private System.Windows.Forms.Label RadiusLabel;
        private System.Windows.Forms.GroupBox PlacementGroupBox;
        private System.Windows.Forms.NumericUpDown ZOffsetNumericUpDown;
        private System.Windows.Forms.Label ZOffsetLabel;
        private System.Windows.Forms.NumericUpDown ScaleMaxNumericUpDown;
        private System.Windows.Forms.NumericUpDown ScaleMinNumericUpDown;
        private System.Windows.Forms.Label ScaleLabel;
        private System.Windows.Forms.NumericUpDown MaxSlopeNumericUpDown;
        private System.Windows.Forms.Label MaxSlopeLabel;
        private System.Windows.Forms.CheckBox AlignToSurfaceCheckBox;
        private System.Windows.Forms.CheckBox RandomYawCheckBox;
        private System.Windows.Forms.Label TargetYmapLabel;
        private System.Windows.Forms.TextBox TargetYmapTextBox;
        private System.Windows.Forms.Label HelpLabel;
        private System.Windows.Forms.Label StatusLabel;
    }
}

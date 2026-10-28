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
            this.ModePropsRadio = new System.Windows.Forms.RadioButton();
            this.ModeGrassRadio = new System.Windows.Forms.RadioButton();
            this.PropsGroupBox = new System.Windows.Forms.GroupBox();
            this.RemoveButton = new System.Windows.Forms.Button();
            this.PropsListBox = new System.Windows.Forms.ListBox();
            this.AddSelectedButton = new System.Windows.Forms.Button();
            this.AddButton = new System.Windows.Forms.Button();
            this.PropNameTextBox = new System.Windows.Forms.TextBox();
            this.GrassGroupBox = new System.Windows.Forms.GroupBox();
            this.GrassTypeLabel = new System.Windows.Forms.Label();
            this.GrassTypeTextBox = new System.Windows.Forms.TextBox();
            this.GrassColorTitleLabel = new System.Windows.Forms.Label();
            this.GrassColorLabel = new System.Windows.Forms.Label();
            this.GrassAoLabel = new System.Windows.Forms.Label();
            this.GrassAoNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.GrassScaleLabel = new System.Windows.Forms.Label();
            this.GrassScaleMinNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.GrassScaleMaxNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.GrassInfoLabel = new System.Windows.Forms.Label();
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
            this.GrassGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GrassAoNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GrassScaleMinNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GrassScaleMaxNumericUpDown)).BeginInit();
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
            this.PaintEnabledCheckBox.Size = new System.Drawing.Size(320, 17);
            this.PaintEnabledCheckBox.TabIndex = 0;
            this.PaintEnabledCheckBox.Text = "&Paint enabled (B) - hold CTRL and left-drag in the world view";
            this.PaintEnabledCheckBox.UseVisualStyleBackColor = true;
            this.PaintEnabledCheckBox.CheckedChanged += new System.EventHandler(this.PaintEnabledCheckBox_CheckedChanged);
            //
            // ModePropsRadio
            //
            this.ModePropsRadio.AutoSize = true;
            this.ModePropsRadio.Checked = true;
            this.ModePropsRadio.Location = new System.Drawing.Point(12, 38);
            this.ModePropsRadio.Name = "ModePropsRadio";
            this.ModePropsRadio.Size = new System.Drawing.Size(80, 17);
            this.ModePropsRadio.TabIndex = 1;
            this.ModePropsRadio.TabStop = true;
            this.ModePropsRadio.Text = "Paint props";
            this.ModePropsRadio.UseVisualStyleBackColor = true;
            this.ModePropsRadio.CheckedChanged += new System.EventHandler(this.ModeRadio_CheckedChanged);
            //
            // ModeGrassRadio
            //
            this.ModeGrassRadio.AutoSize = true;
            this.ModeGrassRadio.Location = new System.Drawing.Point(120, 38);
            this.ModeGrassRadio.Name = "ModeGrassRadio";
            this.ModeGrassRadio.Size = new System.Drawing.Size(80, 17);
            this.ModeGrassRadio.TabIndex = 2;
            this.ModeGrassRadio.Text = "Paint grass (G swaps)";
            this.ModeGrassRadio.UseVisualStyleBackColor = true;
            this.ModeGrassRadio.CheckedChanged += new System.EventHandler(this.ModeRadio_CheckedChanged);
            //
            // PropsGroupBox
            //
            this.PropsGroupBox.Controls.Add(this.RemoveButton);
            this.PropsGroupBox.Controls.Add(this.PropsListBox);
            this.PropsGroupBox.Controls.Add(this.AddSelectedButton);
            this.PropsGroupBox.Controls.Add(this.AddButton);
            this.PropsGroupBox.Controls.Add(this.PropNameTextBox);
            this.PropsGroupBox.Location = new System.Drawing.Point(12, 64);
            this.PropsGroupBox.Name = "PropsGroupBox";
            this.PropsGroupBox.Size = new System.Drawing.Size(406, 170);
            this.PropsGroupBox.TabIndex = 3;
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
            // GrassGroupBox
            //
            this.GrassGroupBox.Controls.Add(this.GrassInfoLabel);
            this.GrassGroupBox.Controls.Add(this.GrassScaleMaxNumericUpDown);
            this.GrassGroupBox.Controls.Add(this.GrassScaleMinNumericUpDown);
            this.GrassGroupBox.Controls.Add(this.GrassScaleLabel);
            this.GrassGroupBox.Controls.Add(this.GrassAoNumericUpDown);
            this.GrassGroupBox.Controls.Add(this.GrassAoLabel);
            this.GrassGroupBox.Controls.Add(this.GrassColorLabel);
            this.GrassGroupBox.Controls.Add(this.GrassColorTitleLabel);
            this.GrassGroupBox.Controls.Add(this.GrassTypeTextBox);
            this.GrassGroupBox.Controls.Add(this.GrassTypeLabel);
            this.GrassGroupBox.Location = new System.Drawing.Point(12, 64);
            this.GrassGroupBox.Name = "GrassGroupBox";
            this.GrassGroupBox.Size = new System.Drawing.Size(406, 170);
            this.GrassGroupBox.TabIndex = 4;
            this.GrassGroupBox.TabStop = false;
            this.GrassGroupBox.Text = "Grass";
            this.GrassGroupBox.Visible = false;
            //
            // GrassTypeLabel
            //
            this.GrassTypeLabel.AutoSize = true;
            this.GrassTypeLabel.Location = new System.Drawing.Point(8, 24);
            this.GrassTypeLabel.Name = "GrassTypeLabel";
            this.GrassTypeLabel.Size = new System.Drawing.Size(60, 13);
            this.GrassTypeLabel.TabIndex = 0;
            this.GrassTypeLabel.Text = "Grass type";
            //
            // GrassTypeTextBox
            //
            this.GrassTypeTextBox.Location = new System.Drawing.Point(100, 21);
            this.GrassTypeTextBox.Name = "GrassTypeTextBox";
            this.GrassTypeTextBox.Size = new System.Drawing.Size(220, 20);
            this.GrassTypeTextBox.TabIndex = 1;
            this.GrassTypeTextBox.Text = "proc_grasses01";
            //
            // GrassColorTitleLabel
            //
            this.GrassColorTitleLabel.AutoSize = true;
            this.GrassColorTitleLabel.Location = new System.Drawing.Point(8, 52);
            this.GrassColorTitleLabel.Name = "GrassColorTitleLabel";
            this.GrassColorTitleLabel.Size = new System.Drawing.Size(31, 13);
            this.GrassColorTitleLabel.TabIndex = 2;
            this.GrassColorTitleLabel.Text = "Tint (click)";
            //
            // GrassColorLabel
            //
            this.GrassColorLabel.BackColor = System.Drawing.Color.White;
            this.GrassColorLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.GrassColorLabel.Location = new System.Drawing.Point(100, 48);
            this.GrassColorLabel.Name = "GrassColorLabel";
            this.GrassColorLabel.Size = new System.Drawing.Size(120, 21);
            this.GrassColorLabel.TabIndex = 3;
            this.GrassColorLabel.Click += new System.EventHandler(this.GrassColorLabel_Click);
            //
            // GrassAoLabel
            //
            this.GrassAoLabel.AutoSize = true;
            this.GrassAoLabel.Location = new System.Drawing.Point(8, 80);
            this.GrassAoLabel.Name = "GrassAoLabel";
            this.GrassAoLabel.Size = new System.Drawing.Size(21, 13);
            this.GrassAoLabel.TabIndex = 4;
            this.GrassAoLabel.Text = "AO";
            //
            // GrassAoNumericUpDown
            //
            this.GrassAoNumericUpDown.Location = new System.Drawing.Point(100, 78);
            this.GrassAoNumericUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.GrassAoNumericUpDown.Name = "GrassAoNumericUpDown";
            this.GrassAoNumericUpDown.Size = new System.Drawing.Size(80, 20);
            this.GrassAoNumericUpDown.TabIndex = 5;
            this.GrassAoNumericUpDown.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            //
            // GrassScaleLabel
            //
            this.GrassScaleLabel.AutoSize = true;
            this.GrassScaleLabel.Location = new System.Drawing.Point(8, 108);
            this.GrassScaleLabel.Name = "GrassScaleLabel";
            this.GrassScaleLabel.Size = new System.Drawing.Size(80, 13);
            this.GrassScaleLabel.TabIndex = 6;
            this.GrassScaleLabel.Text = "Scale (0-255)";
            //
            // GrassScaleMinNumericUpDown
            //
            this.GrassScaleMinNumericUpDown.Location = new System.Drawing.Point(100, 106);
            this.GrassScaleMinNumericUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.GrassScaleMinNumericUpDown.Name = "GrassScaleMinNumericUpDown";
            this.GrassScaleMinNumericUpDown.Size = new System.Drawing.Size(70, 20);
            this.GrassScaleMinNumericUpDown.TabIndex = 7;
            this.GrassScaleMinNumericUpDown.Value = new decimal(new int[] {
            160,
            0,
            0,
            0});
            this.GrassScaleMinNumericUpDown.ValueChanged += new System.EventHandler(this.GrassScaleMinNumericUpDown_ValueChanged);
            //
            // GrassScaleMaxNumericUpDown
            //
            this.GrassScaleMaxNumericUpDown.Location = new System.Drawing.Point(176, 106);
            this.GrassScaleMaxNumericUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.GrassScaleMaxNumericUpDown.Name = "GrassScaleMaxNumericUpDown";
            this.GrassScaleMaxNumericUpDown.Size = new System.Drawing.Size(70, 20);
            this.GrassScaleMaxNumericUpDown.TabIndex = 8;
            this.GrassScaleMaxNumericUpDown.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.GrassScaleMaxNumericUpDown.ValueChanged += new System.EventHandler(this.GrassScaleMaxNumericUpDown_ValueChanged);
            //
            // GrassInfoLabel
            //
            this.GrassInfoLabel.Location = new System.Drawing.Point(8, 134);
            this.GrassInfoLabel.Name = "GrassInfoLabel";
            this.GrassInfoLabel.Size = new System.Drawing.Size(390, 32);
            this.GrassInfoLabel.TabIndex = 9;
            this.GrassInfoLabel.Text = "Grass is added to nearby batches of the same type in the selected ymap. New batches are created automatically as you paint.";
            //
            // BrushGroupBox
            //
            this.BrushGroupBox.Controls.Add(this.SpacingNumericUpDown);
            this.BrushGroupBox.Controls.Add(this.SpacingLabel);
            this.BrushGroupBox.Controls.Add(this.CountNumericUpDown);
            this.BrushGroupBox.Controls.Add(this.CountLabel);
            this.BrushGroupBox.Controls.Add(this.RadiusNumericUpDown);
            this.BrushGroupBox.Controls.Add(this.RadiusLabel);
            this.BrushGroupBox.Location = new System.Drawing.Point(12, 240);
            this.BrushGroupBox.Name = "BrushGroupBox";
            this.BrushGroupBox.Size = new System.Drawing.Size(200, 150);
            this.BrushGroupBox.TabIndex = 5;
            this.BrushGroupBox.TabStop = false;
            this.BrushGroupBox.Text = "Brush ( [ and ] resize )";
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
            this.CountLabel.Text = "Items per dab";
            //
            // CountNumericUpDown
            //
            this.CountNumericUpDown.Location = new System.Drawing.Point(110, 48);
            this.CountNumericUpDown.Maximum = new decimal(new int[] {
            200,
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
            this.SpacingNumericUpDown.DecimalPlaces = 2;
            this.SpacingNumericUpDown.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
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
            this.PlacementGroupBox.Location = new System.Drawing.Point(218, 240);
            this.PlacementGroupBox.Name = "PlacementGroupBox";
            this.PlacementGroupBox.Size = new System.Drawing.Size(200, 150);
            this.PlacementGroupBox.TabIndex = 6;
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
            this.TargetYmapLabel.Location = new System.Drawing.Point(12, 401);
            this.TargetYmapLabel.Name = "TargetYmapLabel";
            this.TargetYmapLabel.Size = new System.Drawing.Size(80, 13);
            this.TargetYmapLabel.TabIndex = 7;
            this.TargetYmapLabel.Text = "Paint into YMAP";
            //
            // TargetYmapTextBox
            //
            this.TargetYmapTextBox.Location = new System.Drawing.Point(110, 398);
            this.TargetYmapTextBox.Name = "TargetYmapTextBox";
            this.TargetYmapTextBox.ReadOnly = true;
            this.TargetYmapTextBox.Size = new System.Drawing.Size(308, 20);
            this.TargetYmapTextBox.TabIndex = 8;
            this.TargetYmapTextBox.TabStop = false;
            //
            // HelpLabel
            //
            this.HelpLabel.Location = new System.Drawing.Point(12, 426);
            this.HelpLabel.Name = "HelpLabel";
            this.HelpLabel.Size = new System.Drawing.Size(406, 62);
            this.HelpLabel.TabIndex = 9;
            this.HelpLabel.Text = "CTRL + left-drag: paint. CTRL + SHIFT + left-drag: erase (red circle).\r\nB: paint on/off. G: swap props/grass. [ and ] (or CTRL + mouse wheel): brush size.";
            //
            // StatusLabel
            //
            this.StatusLabel.AutoEllipsis = true;
            this.StatusLabel.Location = new System.Drawing.Point(12, 490);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(406, 18);
            this.StatusLabel.TabIndex = 10;
            //
            // PaintPropsPanel
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 516);
            this.Controls.Add(this.StatusLabel);
            this.Controls.Add(this.HelpLabel);
            this.Controls.Add(this.TargetYmapTextBox);
            this.Controls.Add(this.TargetYmapLabel);
            this.Controls.Add(this.PlacementGroupBox);
            this.Controls.Add(this.BrushGroupBox);
            this.Controls.Add(this.GrassGroupBox);
            this.Controls.Add(this.PropsGroupBox);
            this.Controls.Add(this.ModeGrassRadio);
            this.Controls.Add(this.ModePropsRadio);
            this.Controls.Add(this.PaintEnabledCheckBox);
            this.Name = "PaintPropsPanel";
            this.Text = "Paint Props / Grass";
            this.PropsGroupBox.ResumeLayout(false);
            this.PropsGroupBox.PerformLayout();
            this.GrassGroupBox.ResumeLayout(false);
            this.GrassGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GrassAoNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GrassScaleMinNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GrassScaleMaxNumericUpDown)).EndInit();
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
        private System.Windows.Forms.RadioButton ModePropsRadio;
        private System.Windows.Forms.RadioButton ModeGrassRadio;
        private System.Windows.Forms.GroupBox PropsGroupBox;
        private System.Windows.Forms.Button RemoveButton;
        private System.Windows.Forms.ListBox PropsListBox;
        private System.Windows.Forms.Button AddSelectedButton;
        private System.Windows.Forms.Button AddButton;
        private System.Windows.Forms.TextBox PropNameTextBox;
        private System.Windows.Forms.GroupBox GrassGroupBox;
        private System.Windows.Forms.Label GrassTypeLabel;
        private System.Windows.Forms.TextBox GrassTypeTextBox;
        private System.Windows.Forms.Label GrassColorTitleLabel;
        private System.Windows.Forms.Label GrassColorLabel;
        private System.Windows.Forms.Label GrassAoLabel;
        private System.Windows.Forms.NumericUpDown GrassAoNumericUpDown;
        private System.Windows.Forms.Label GrassScaleLabel;
        private System.Windows.Forms.NumericUpDown GrassScaleMinNumericUpDown;
        private System.Windows.Forms.NumericUpDown GrassScaleMaxNumericUpDown;
        private System.Windows.Forms.Label GrassInfoLabel;
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

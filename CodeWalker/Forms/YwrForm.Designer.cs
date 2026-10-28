namespace CodeWalker.Forms
{
    partial class YwrForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(YwrForm));
            this.CloseButton = new System.Windows.Forms.Button();
            this.MainListView = new System.Windows.Forms.ListView();
            this.XPosColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.YPosColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ZPosColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Flags0Column = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Flags1Column = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ExportButton = new System.Windows.Forms.Button();
            this.CopyClipboardButton = new System.Windows.Forms.Button();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // CloseButton
            // 
            this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CloseButton.Location = new System.Drawing.Point(417, 389);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(75, 23);
            this.CloseButton.TabIndex = 3;
            this.CloseButton.Text = "Close";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // MainListView
            // 
            this.MainListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.XPosColumn,
            this.YPosColumn,
            this.ZPosColumn,
            this.Flags0Column,
            this.Flags1Column});
            this.MainListView.HideSelection = false;
            this.MainListView.Location = new System.Drawing.Point(13, 13);
            this.MainListView.Name = "MainListView";
            this.MainListView.Size = new System.Drawing.Size(479, 370);
            this.MainListView.TabIndex = 4;
            this.MainListView.UseCompatibleStateImageBehavior = false;
            this.MainListView.View = System.Windows.Forms.View.Details;
            // 
            // XPosColumn
            // 
            this.XPosColumn.Text = "Position X";
            this.XPosColumn.Width = 75;
            // 
            // YPosColumn
            // 
            this.YPosColumn.Text = "Position Y";
            this.YPosColumn.Width = 75;
            // 
            // ZPosColumn
            // 
            this.ZPosColumn.Text = "Position Z";
            this.ZPosColumn.Width = 75;
            // 
            // Flags0Column
            //
            this.Flags0Column.Text = "Flags0";
            this.Flags0Column.Width = 120;
            //
            // Flags1Column
            //
            this.Flags1Column.Text = "Flags1";
            this.Flags1Column.Width = 100;
            // 
            // ExportButton
            // 
            this.ExportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ExportButton.Enabled = false;
            this.ExportButton.Location = new System.Drawing.Point(336, 389);
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Size = new System.Drawing.Size(75, 23);
            this.ExportButton.TabIndex = 5;
            this.ExportButton.Text = "Export";
            this.ExportButton.UseVisualStyleBackColor = true;
            this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // CopyClipboardButton
            // 
            this.CopyClipboardButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CopyClipboardButton.Enabled = false;
            this.CopyClipboardButton.Location = new System.Drawing.Point(227, 389);
            this.CopyClipboardButton.Name = "CopyClipboardButton";
            this.CopyClipboardButton.Size = new System.Drawing.Size(103, 23);
            this.CopyClipboardButton.TabIndex = 6;
            this.CopyClipboardButton.Text = "Copy to clipboard";
            this.CopyClipboardButton.UseVisualStyleBackColor = true;
            this.CopyClipboardButton.Click += new System.EventHandler(this.CopyClipboardButton_Click);
            // 
            // YwrForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CloseButton;
            this.ClientSize = new System.Drawing.Size(504, 421);
            this.Controls.Add(this.CopyClipboardButton);
            this.Controls.Add(this.ExportButton);
            this.Controls.Add(this.MainListView);
            this.Controls.Add(this.CloseButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "YwrForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Waypoint Records Viewer - CodeWalker by dexyfex";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.ListView MainListView;
        private System.Windows.Forms.Button ExportButton;
        private System.Windows.Forms.Button CopyClipboardButton;
        private System.Windows.Forms.ColumnHeader XPosColumn;
        private System.Windows.Forms.ColumnHeader YPosColumn;
        private System.Windows.Forms.ColumnHeader ZPosColumn;
        private System.Windows.Forms.ColumnHeader Flags0Column;
        private System.Windows.Forms.ColumnHeader Flags1Column;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
    }
}
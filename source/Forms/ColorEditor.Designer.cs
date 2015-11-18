namespace TextEditor2.Forms
{
    partial class ColorEditor
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
            this.ColorPicBox = new System.Windows.Forms.PictureBox();
            this.ColorCombo = new System.Windows.Forms.ComboBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ColorTextBox = new System.Windows.Forms.RichTextBox();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            ((System.ComponentModel.ISupportInitialize)(this.ColorPicBox)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ColorPicBox
            // 
            this.ColorPicBox.Location = new System.Drawing.Point(12, 65);
            this.ColorPicBox.Name = "ColorPicBox";
            this.ColorPicBox.Size = new System.Drawing.Size(112, 112);
            this.ColorPicBox.TabIndex = 0;
            this.ColorPicBox.TabStop = false;
            this.ColorPicBox.Click += new System.EventHandler(this.ColorPicBox_Click);
            // 
            // ColorCombo
            // 
            this.ColorCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ColorCombo.FormattingEnabled = true;
            this.ColorCombo.IntegralHeight = false;
            this.ColorCombo.Location = new System.Drawing.Point(12, 38);
            this.ColorCombo.Name = "ColorCombo";
            this.ColorCombo.Size = new System.Drawing.Size(112, 21);
            this.ColorCombo.TabIndex = 1;
            this.ColorCombo.SelectedIndexChanged += new System.EventHandler(this.ColorCombo_SelectedIndexChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(242, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // ColorTextBox
            // 
            this.ColorTextBox.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ColorTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ColorTextBox.Location = new System.Drawing.Point(131, 65);
            this.ColorTextBox.Name = "ColorTextBox";
            this.ColorTextBox.ReadOnly = true;
            this.ColorTextBox.Size = new System.Drawing.Size(99, 112);
            this.ColorTextBox.TabIndex = 3;
            this.ColorTextBox.Text = "The quick brown fox jumps over the lazy dog.";
            // 
            // ColorEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(242, 186);
            this.Controls.Add(this.ColorTextBox);
            this.Controls.Add(this.ColorCombo);
            this.Controls.Add(this.ColorPicBox);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ColorEditor";
            this.Text = "Color Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ColorEditor_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.ColorPicBox)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox ColorPicBox;
        private System.Windows.Forms.ComboBox ColorCombo;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.RichTextBox ColorTextBox;
        private System.Windows.Forms.ColorDialog colorDialog1;
    }
}
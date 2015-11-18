namespace TextEditor2.Forms
{
    partial class SearchForm
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
            this.TextRadioButton = new System.Windows.Forms.RadioButton();
            this.IDRadioButton = new System.Windows.Forms.RadioButton();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.TextSearchBox = new System.Windows.Forms.TextBox();
            this.IDSearchUpDown = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.IDSearchUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // TextRadioButton
            // 
            this.TextRadioButton.AutoSize = true;
            this.TextRadioButton.Checked = true;
            this.TextRadioButton.Location = new System.Drawing.Point(12, 47);
            this.TextRadioButton.Name = "TextRadioButton";
            this.TextRadioButton.Size = new System.Drawing.Size(46, 17);
            this.TextRadioButton.TabIndex = 0;
            this.TextRadioButton.TabStop = true;
            this.TextRadioButton.Text = "Text";
            this.TextRadioButton.UseVisualStyleBackColor = true;
            this.TextRadioButton.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // IDRadioButton
            // 
            this.IDRadioButton.AutoSize = true;
            this.IDRadioButton.Location = new System.Drawing.Point(83, 47);
            this.IDRadioButton.Name = "IDRadioButton";
            this.IDRadioButton.Size = new System.Drawing.Size(82, 17);
            this.IDRadioButton.TabIndex = 1;
            this.IDRadioButton.Text = "Message ID";
            this.IDRadioButton.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 70);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Search";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(93, 70);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // TextSearchBox
            // 
            this.TextSearchBox.Location = new System.Drawing.Point(12, 21);
            this.TextSearchBox.Name = "TextSearchBox";
            this.TextSearchBox.Size = new System.Drawing.Size(100, 20);
            this.TextSearchBox.TabIndex = 4;
            // 
            // IDSearchUpDown
            // 
            this.IDSearchUpDown.Location = new System.Drawing.Point(12, 21);
            this.IDSearchUpDown.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.IDSearchUpDown.Name = "IDSearchUpDown";
            this.IDSearchUpDown.Size = new System.Drawing.Size(100, 20);
            this.IDSearchUpDown.TabIndex = 5;
            this.IDSearchUpDown.Visible = false;
            // 
            // SearchForm
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(174, 100);
            this.Controls.Add(this.IDSearchUpDown);
            this.Controls.Add(this.TextSearchBox);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.IDRadioButton);
            this.Controls.Add(this.TextRadioButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "SearchForm";
            this.Text = "Search";
            ((System.ComponentModel.ISupportInitialize)(this.IDSearchUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton TextRadioButton;
        private System.Windows.Forms.RadioButton IDRadioButton;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox TextSearchBox;
        private System.Windows.Forms.NumericUpDown IDSearchUpDown;
    }
}
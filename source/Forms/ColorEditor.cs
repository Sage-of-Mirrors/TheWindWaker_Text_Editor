using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TextEditor2.Forms
{
    public partial class ColorEditor : Form
    {
        public List<ByteColorAlpha> ColorList = new List<ByteColorAlpha>();
        public int SelectedColorIndex;
        public bool HasEdited;

        public delegate void SaveColorEdits(List<ByteColorAlpha> editedColorList);

        public event SaveColorEdits OnColorListSaved;

        public ColorEditor(List<ByteColorAlpha> colorList)
        {
            InitializeComponent();

            CopyColorList(colorList);

            FillUI();
        }

        protected void OnSaveColors(List<ByteColorAlpha> editList)
        {
            if (OnColorListSaved != null)
            {
                OnColorListSaved(editList);
            }
        }

        private void CopyColorList(List<ByteColorAlpha> sourceList)
        {
            foreach (ByteColorAlpha byteColor in sourceList)
            {
                byte[] temp = byteColor.GetBytes();

                ByteColorAlpha copiedColor = new ByteColorAlpha(temp);

                ColorList.Add(copiedColor);
            }
        }

        private void FillUI()
        {
            ColorCombo.Items.Clear();

            for (int i = 0; i < 256; i++)
            {
                ColorCombo.Items.Add(i);
            }

            ColorCombo.SelectedIndex = 0;
        }

        private void UpdateUI()
        {
            Color currentColor = Color.FromArgb((int)ColorList[SelectedColorIndex].R, (int)ColorList[SelectedColorIndex].G,
                (int)ColorList[SelectedColorIndex].B);

            ColorPicBox.BackColor = currentColor;

            ColorTextBox.ForeColor = currentColor;
        }

        private void EditColor()
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                HasEdited = true;

                ColorList[SelectedColorIndex].R = colorDialog1.Color.R;
                ColorList[SelectedColorIndex].G = colorDialog1.Color.G;
                ColorList[SelectedColorIndex].B = colorDialog1.Color.B;

                UpdateUI();
            }
        }

        private void ColorCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedColorIndex = ColorCombo.SelectedIndex;
            UpdateUI();
        }

        private void ColorPicBox_Click(object sender, EventArgs e)
        {
            EditColor();
        }

        private void ColorEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!HasEdited)
                return;
            
            DialogResult result = MessageBox.Show("There are unsaved colors. Would you like to save them?", "Warning",
                MessageBoxButtons.YesNoCancel);

            switch (result)
            {
                case DialogResult.Yes:
                    OnSaveColors(ColorList);
                    break;
                case DialogResult.Cancel:
                    e.Cancel = true;
                    break;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnSaveColors(ColorList);

            HasEdited = false;
        }
    }
}

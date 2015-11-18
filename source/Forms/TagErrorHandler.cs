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
    public partial class TagErrorHandler : Form
    {
        public List<byte> ErrorData;
        public TagErrorHandler(string title, string error, List<byte> errorData)
        {
            InitializeComponent();

            ControlBox = false;

            this.Text = title;

            ErrorDescription.Text = error;

            ErrorData = errorData;

            FillTextBox();
        }

        private void FillTextBox()
        {
            ErrorTextBox.Text = new string(Encoding.ASCII.GetChars(ErrorData.ToArray()));
        }

        public List<byte> GetCorrectedData()
        {
            ErrorData = new List<byte>(Encoding.ASCII.GetBytes(ErrorTextBox.Text.ToCharArray()));

            return ErrorData;
        }
    }
}

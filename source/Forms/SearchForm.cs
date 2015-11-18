using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TextEditor2;

namespace TextEditor2.Forms
{
    public partial class SearchForm : Form
    {
        public bool IsTextSearch = true;
        public List<Message> MessageList;

        public int StartingIndex = -1;

        public delegate void MessageIndexFound(int Index);

        public event MessageIndexFound IndexFound;

        protected void OnIndexFound(int Index)
        {
            if (IndexFound != null)
            {
                IndexFound(Index);
            }
        }

        public SearchForm(List<Message> messageList, MainUI uI)
        {
            InitializeComponent();

            MessageList = messageList;

            uI.OnMessageIndexChanged += new MainUI.SelectedMessageIndexChanged(form1_IndexChanged);
        }

        void form1_IndexChanged(int Index)
        {
            StartingIndex = Index;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = TextRadioButton.Checked;

            if (isChecked)
            {
                IsTextSearch = true;

                IDSearchUpDown.Visible = false;

                TextSearchBox.Visible = true;
            }

            else
            {
                IsTextSearch = false;

                TextSearchBox.Visible = false;

                IDSearchUpDown.Visible = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (IsTextSearch == true)
            {
                TextSearch();
            }

            else
            {
                IDSearch();
            }
        }

        private void TextSearch()
        {
            if (TextSearchBox.Text != "")
            {
                for (int i = StartingIndex + 1; i <= MessageList.Count - 1; i++)
                {
                    string source = new string(Encoding.ASCII.GetChars(MessageList[i].charData.ToArray()));

                    bool contains = source.Contains(TextSearchBox.Text);

                    if (contains == true)
                    {
                        OnIndexFound(i);

                        return;
                    }
                }

                DialogResult result = MessageBox.Show("String not found. Restart from the beginning?", "String not found", MessageBoxButtons.OKCancel);

                if (result == DialogResult.OK)
                {
                    StartingIndex = -1;

                    TextSearch();

                    return;
                }
            }
        }

        private void IDSearch()
        {
            for (int i = StartingIndex + 1; i <= MessageList.Count - 1; i++)
            {
                if (MessageList[i].msgID == (short)IDSearchUpDown.Value)
                {
                    OnIndexFound(i);

                    return;
                }
            }

            DialogResult result = MessageBox.Show("Message ID was not found. Restart from the beginning?", "Message ID not found", MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK)
            {
                StartingIndex = 0;

                IDSearch();

                return;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using GameFormatReader.Common;
using GameFormatReader.GCWii.Binaries.GC;
using GameFormatReader.GCWii.Compression;
using TextEditor2.Forms;

namespace TextEditor2
{
    public partial class MainUI : Form
    {
        #region Variables

        public List<Message> MessageList;
        public List<ByteColorAlpha> ColorList;

        public bool IsBMGLoaded;
        public bool IsBMCLoaded;
        public bool IsSearchByText = true;
        public bool Inhibit;

        public string SaveSearchText = "";
        public string ArcFilePath = "";
        public string LastSearchTerm = "";

        public int SaveSearchID = -1;
        public int SelectedMessageIndex = 0;

        RARC.FileEntry TextBankBMG;
        RARC.FileEntry TextColorBMC;

        BMG_New TextBankClass;

        BMCParser ColorClass;

        public delegate void SelectedMessageIndexChanged(int Index);

        public event SelectedMessageIndexChanged OnMessageIndexChanged;

        #endregion

        protected void OnIndexChanged(int Index)
        {
            if (OnMessageIndexChanged != null)
            {
                OnMessageIndexChanged(Index);
            }
        }

        #region Main

        public MainUI()
        {
            InitializeComponent();

            AddSearchTextBox();

            searchBoxBackPanel.Controls[0].Enabled = false;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void OpenFile()
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                IsBMGLoaded = false;

                IsBMCLoaded = false;

                if (Yaz0.IsYaz0Compressed(openFileDialog1.FileName))
                {
                    MessageBox.Show("Archive is compressed. Please decompress it.");

                    return;
                }

                RARC archive = new RARC(openFileDialog1.FileName);

                int numRARCNodes = archive.Nodes.Count();

                if (numRARCNodes < 0)
                    return;

                foreach (RARC.FileEntry entry in archive.Nodes[0].Entries)
                {
                    if (entry.Name.EndsWith(".bmg"))
                    {
                        TextBankBMG = entry;

                        TextBankClass = new BMG_New(new EndianBinaryReader(entry.Data, Endian.Big));

                        if (TextBankClass.IsWindWaker == false)
                        {
                            MessageBox.Show("The BMG file in this archive is not from The Wind Waker. BMG files from other games are not supported at this time.");

                            return;
                        }

                        ArcFilePath = openFileDialog1.FileName;

                        saveFileDialog1.InitialDirectory = ArcFilePath;

                        IsBMGLoaded = true;

                        MessageList = TextBankClass.GetMessageList();

                        saveToolStripMenuItem.Enabled = true;
                        AddToolStrip.Enabled = true;
                        deleteToolStripMenuItem.Enabled = true;
                        proofreadToolStripMenuItem.Enabled = true;
                        searchBoxBackPanel.Controls[0].Enabled = true;
                        SearchButton.Enabled = true;
                        textOpt.Enabled = true;
                        idOpt.Enabled = true;

                        LoadMainUI();
                    }

                    if (entry.Name.EndsWith(".bmc"))
                    {
                        TextColorBMC = entry;
                        IsBMCLoaded = true;
                        textColorsToolStripMenuItem.Enabled = true;

                        ColorClass = new BMCParser(new EndianBinaryReader(entry.Data, Endian.Big));

                        ColorList = ColorClass.GetColorList();
                    }
                }

                if (!IsBMGLoaded && IsBMCLoaded)
                    MessageBox.Show("Archive does not contain a text bank (.bmg).");

                if (IsBMGLoaded && !IsBMCLoaded)
                    MessageBox.Show("Archive does not contain a text color file (.bmc).");

                if (!IsBMGLoaded && !IsBMCLoaded)
                    MessageBox.Show("Archive does not contain a text bank (.bmg) or a text color file (.bmc).");
            }

            return;
        }

        #endregion

        #region UI Filling Functions

        private void LoadMainUI()
        {
            IsBMGLoaded = true;

            IsSearchByText = true;

            FillMessageListBox();

            MessageListBox.SelectedIndex = 0;

            MessageText.Enabled = true;

            IndexBox.Enabled = true;
        }

        private void FillMessageListBox()
        {
            MessageListBox.BeginUpdate();

            MessageListBox.Items.Clear();

            foreach (Message mes in MessageList)
            {
                MessageListBox.Items.Add(mes.msgID + ". " + new string(Encoding.ASCII.GetChars(mes.charData.ToArray())));
            }

            MessageListBox.Enabled = true;

            MessageListBox.EndUpdate();
        }

        private void FillMessageTextBox()
        {
            MessageText.Clear();

            Inhibit = true;

            MessageText.Text = new string(Encoding.ASCII.GetChars(MessageList[SelectedMessageIndex].charData.ToArray()));

            MessageText.Text = MessageText.Text.Replace("\n", Environment.NewLine);

            MessageListBox.Items[SelectedMessageIndex] = MessageList[SelectedMessageIndex].msgID + ". " + MessageText.Text;

            Inhibit = false;
        }

        private void FillBoxInfo()
        {
            DrawTypeBox.Enabled = true;
            DrawTypeBox.Text = Convert.ToString(MessageList[SelectedMessageIndex].drawType);

            LinesUpDown.Enabled = true;
            LinesUpDown.Value = MessageList[SelectedMessageIndex].maxLines;

            ItemIDUpDown.Enabled = true;
            if (MessageList[SelectedMessageIndex].boxType != 0x9)
            {
                ItemIDUpDown.Enabled = false;
            }
            ItemIDUpDown.Value = MessageList[SelectedMessageIndex].itemID;

            PosCombo.Enabled = true;
            switch (MessageList[SelectedMessageIndex].boxPos)
            {
                case 0x0:
                case 0x1:
                    PosCombo.SelectedIndex = 0;
                    break;
                case 0x2:
                    PosCombo.SelectedIndex = 1;
                    break;
                case 0x3:
                case 0x4:
                    PosCombo.SelectedIndex = 2;
                    break;
                default:
                    MessageBox.Show("Found Position " + MessageList[SelectedMessageIndex].boxPos);
                    break;
            }

            TypeCombo.Enabled = true;
            switch (MessageList[SelectedMessageIndex].boxType)
            {
                case 0x0:
                    TypeCombo.SelectedIndex = 0;
                    break;
                case 0x1:
                    TypeCombo.SelectedIndex = 1;
                    break;
                case 0x2:
                    TypeCombo.SelectedIndex = 2;
                    break;
                case 0x3:
                case 0x4:
                    TypeCombo.SelectedIndex = 0;
                    break;
                case 0x5:
                    TypeCombo.SelectedIndex = 3;
                    break;
                case 0x6:
                    TypeCombo.SelectedIndex = 4;
                    break;
                case 0x7:
                    TypeCombo.SelectedIndex = 5;
                    break;
                case 0x8:
                    TypeCombo.SelectedIndex = 0;
                    break;
                case 0x9:
                    TypeCombo.SelectedIndex = 6;
                    break;
                case 0xA:
                    TypeCombo.SelectedIndex = 7;
                    break;
                case 0x0D:
                    TypeCombo.SelectedIndex = 8;
                    break;
                case 0x0E:
                    TypeCombo.SelectedIndex = 9;
                    break;
                case 0x0C:
                    TypeCombo.SelectedIndex = 10;
                    break;
                case 0x0B:
                    TypeCombo.SelectedIndex = 7;
                    break;
                default:
                    MessageBox.Show("Found Type " + MessageList[SelectedMessageIndex].boxType);
                    break;
            }
        }

        private void MessageListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Inhibit)
                return;

            UpdateSelectedMessage(MessageListBox.SelectedIndex);
        }

        private void IndexBox_ValueChanged(object sender, EventArgs e)
        {
            if (IndexBox.Value <= MessageList.Count - 1)
            {
                if (Inhibit)
                    return;

                UpdateSelectedMessage((int)IndexBox.Value);
            }

            else
            {
                MessageBox.Show("Desired index is out of range.", "Index out of Range");

                IndexBox.Value = SelectedMessageIndex;
            }
        }

        private void UpdateSelectedMessage(int index)
        {
            SelectedMessageIndex = index;

            Inhibit = true;

            MessageListBox.SelectedIndex = index;

            IndexBox.Value = index;

            OnIndexChanged(SelectedMessageIndex);

            ChangeTextAndBoxInfo();

            Inhibit = false;
        }

        private void ChangeTextAndBoxInfo()
        {
            FillMessageTextBox();

            FillBoxInfo();
        }
        #endregion

        #region Change-Storing Functions

        private void MessageText_TextChanged(object sender, EventArgs e)
        {
            if (Inhibit)
            {
                return;
            }

            MessageList[SelectedMessageIndex].charData = new List<byte>(Encoding.ASCII.GetBytes(MessageText.Text.ToCharArray()));

            Inhibit = true;

            MessageListBox.Items[SelectedMessageIndex] = MessageList[SelectedMessageIndex].msgID + ". " + MessageText.Text;

            Inhibit = false;
        }

        private void PosCombo_SelectionChangeCommitted(object sender, EventArgs e)
        {
            switch (PosCombo.SelectedIndex)
            {
                case 0:
                    MessageList[SelectedMessageIndex].boxPos = 0;
                    break;
                case 1:
                    MessageList[SelectedMessageIndex].boxPos = 2;
                    break;
                case 2:
                    MessageList[SelectedMessageIndex].boxPos = 3;
                    break;
            }
        }

        private void TypeCombo_SelectionChangeCommitted(object sender, EventArgs e)
        {
            switch (TypeCombo.SelectedIndex)
            {
                case 0:
                    MessageList[SelectedMessageIndex].boxType = 0;
                    break;
                case 1:
                    MessageList[SelectedMessageIndex].boxType = 1;
                    break;
                case 2:
                    MessageList[SelectedMessageIndex].boxType = 2;
                    break;
                case 3:
                    MessageList[SelectedMessageIndex].boxType = 5;
                    break;
                case 4:
                    MessageList[SelectedMessageIndex].boxType = 6;
                    break;
                case 5:
                    MessageList[SelectedMessageIndex].boxType = 7;
                    break;
                case 6:
                    MessageList[SelectedMessageIndex].boxType = 9;
                    break;
                case 7:
                    MessageList[SelectedMessageIndex].boxType = 0x0A;
                    break;
                case 8:
                    MessageList[SelectedMessageIndex].boxType = 0x0D;
                    break;
                case 9:
                    MessageList[SelectedMessageIndex].boxType = 0x0E;
                    break;
                case 10:
                    MessageList[SelectedMessageIndex].boxType = 0x0C;
                    break;
            }

            FillBoxInfo();
        }

        private void LinesUpDown_ValueChanged(object sender, EventArgs e)
        {
            MessageList[SelectedMessageIndex].maxLines = (byte)LinesUpDown.Value;
        }

        private void ItemIDUpDown_ValueChanged(object sender, EventArgs e)
        {
            MessageList[SelectedMessageIndex].itemID = (byte)ItemIDUpDown.Value;
        }

        private void SaveFiles()
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string savePath = saveFileDialog1.FileName;

                FileStream originalArc = new FileStream(ArcFilePath, FileMode.Open);

                EndianBinaryReader reader = new EndianBinaryReader(originalArc, GameFormatReader.Common.Endian.Big);

                List<byte> testList = reader.ReadBytes((int)reader.BaseStream.Length).ToList();

                reader.Close();

                List<Message> exportList = new List<Message>();

                foreach (Message mes in MessageList)
                {
                    mes.ProofReadTagstoCodes();

                    Message temp = mes.Copy();

                    exportList.Add(temp);
                }

                byte[] newBMGFile = TextBankClass.ExportBMGFromPath(exportList);

                testList.RemoveRange(1344, testList.Count - 1344);

                testList.AddRange(newBMGFile);

                byte[] newBMCFile = ColorClass.BMCExporter(ColorList);

                testList.RemoveRange(256, newBMCFile.Length);

                testList.InsertRange(256, newBMCFile);

                FileStream testStream = new FileStream(savePath, FileMode.Create);

                EndianBinaryWriter writer = new EndianBinaryWriter(testStream, Endian.Big);

                writer.Write(testList.ToArray());

                writer.BaseStream.Position = 128;

                writer.Write((int)newBMGFile.Length);

                writer.BaseStream.Position = 4;

                writer.Write((int)testList.Count);

                writer.Flush();

                writer.Close();
            }
        }

        #endregion

        #region Adding and Deleting Messages

        private void AddNewMessage()
        {
            Message newMessage = new Message();

            newMessage.msgID = (ushort)GetHighestMsgID();

            newMessage.msgID += 1;

            MessageList.Add(newMessage);

            FillMessageListBox();

            UpdateSelectedMessage(MessageList.Count - 1);
        }

        private int GetHighestMsgID()
        {
            int ID = 0;

            for (int i = 0; i <= MessageList.Count - 1; i++)
            {
                if (i + 1 > MessageList.Count - 1)
                {
                    break;
                }

                if (MessageList[i + 1].msgID > ID)
                {
                    ID = MessageList[i + 1].msgID;
                }
            }

            return ID;
        }

        private void DeleteMessage()
        {
            if (MessageList.Count == 1)
            {
                MessageBox.Show("There must be at least one message in the file.");

                return;
            }

            MessageList.RemoveAt(SelectedMessageIndex);

            FillMessageListBox();

            if (SelectedMessageIndex >= MessageList.Count)
            {
                UpdateSelectedMessage(SelectedMessageIndex - 1);

                return;
            }

            UpdateSelectedMessage(SelectedMessageIndex);
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DeleteMessage();
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddNewMessage();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteMessage();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFiles();
        }

        private void MessageListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                MessageListBox.SelectedIndex = MessageListBox.IndexFromPoint(e.Location);

                if (MessageListBox.SelectedIndex != -1)
                {
                    MessageListBoxMenu.Show(MessageListBox, e.Location);
                }
            }
        }

        #endregion

        #region ContextMenu Options

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageText.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageText.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageText.Paste();
        }

        #region Control Code Insertion

        private void controlTagsToolStripMenuItem1_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            AddPlayerColorScaleTags(e.ClickedItem.Text);
        }

        /// <summary>
        /// Handles the three tag inserts at TextBoxMenu/Control Tags. They are the player's name, text color, and text size.
        /// </summary>
        /// <param name="tag"></param>
        private void AddPlayerColorScaleTags(string text)
        {
            int selectionStart = MessageText.SelectionStart;

            string tempTag = "";

            switch (text)
            {
                case "Player Name":
                    tempTag = "<Player>";
                    break;
                case "Color":
                    tempTag = "<Color:0>";
                    break;
                case "Scale":
                    tempTag = "<Scale:100>";
                    break;
            }

            MessageText.Text = MessageText.Text.Insert(MessageText.SelectionStart, tempTag);

            MessageText.SelectionStart = selectionStart + tempTag.Length;
        }

        private void controllerButtonsToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            AddMainControllerTags(e.ClickedItem.Text);
        }

        /// <summary>
        /// This handles the tag inserts at TextBoxMenu/Control Tags/Controller Buttons. They are button icons from the Gamecube's controller.
        /// </summary>
        /// <param name="tag"></param>
        private void AddMainControllerTags(string text)
        {
            int selectionStart = MessageText.SelectionStart;

            string tempTag = "";

            switch (text)
            {
                case "A Button":
                    tempTag = "<A Button>";
                    break;
                case "Starburst A Button":
                    tempTag = "<Starburst A Button>";
                    break;
                case "B Button":
                    tempTag = "<B Button>";
                    break;
                case "C-Stick":
                    tempTag = "<C-Stick>";
                    break;
                case "D Pad":
                    tempTag = "<D Pad>";
                    break;
                case "L Trigger":
                    tempTag = "<L Trigger>";
                    break;
                case "R Trigger":
                    tempTag = "<R Trigger>";
                    break;
                case "X Button":
                    tempTag = "<X Button>";
                    break;
                case "Y Button":
                    tempTag = "<Y Button>";
                    break;
                case "Z Button":
                    tempTag = "<Z Button>";
                    break;
            }

            MessageText.Text = MessageText.Text.Insert(MessageText.SelectionStart, tempTag);

            MessageText.SelectionStart = selectionStart + tempTag.Length;
        }

        private void controlStickToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            AddControlStickTags(e.ClickedItem.Text);
        }

        /// <summary>
        /// This handles the tag inserts at TextBoxMenu/Control Tags/Controller Buttons/Control Stick.
        /// They are the Gamecube Controller's control stick's static icon along with its various animated variants.
        /// </summary>
        /// <param name="text"></param>
        private void AddControlStickTags(string text)
        {
            int selectionStart = MessageText.SelectionStart;

            string tempTag = "";

            switch (text)
            {
                case "Static":
                    tempTag = "<Control Stick>";
                    break;
                case "Moving Up":
                    tempTag = "<Control Stick:Up>";
                    break;
                case "Moving Down":
                    tempTag = "<Control Stick:Down>";
                    break;
                case "Moving Left":
                    tempTag = "<Control Stick:Left>";
                    break;
                case "Moving Right":
                    tempTag = "<Control Stick:Right>";
                    break;
                case "Moving Up/Down":
                    tempTag = "<Control Stick:Up+Down>";
                    break;
                case "Moving Left/Right":
                    tempTag = "<Control Stick:Left+Right>";
                    break;
            }

            MessageText.Text = MessageText.Text.Insert(MessageText.SelectionStart, tempTag);

            MessageText.SelectionStart = selectionStart + tempTag.Length;
        }

        private void iconsToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            AddIconTags(e.ClickedItem.Text);
        }

        /// <summary>
        /// This handles the tag inserts at TextBoxMenu/Control Tags/Icons.
        /// They are the heart, music note, and target starburst icons.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddIconTags(string text)
        {
            int selectionStart = MessageText.SelectionStart;

            string tempTag = "";

            switch (text)
            {
                case "Heart":
                    tempTag = "<Heart Icon>";
                    break;
                case "Music Note":
                    tempTag = "<Music Note Icon>";
                    break;
                case "Target Starburst":
                    tempTag = "<Target Starburst>";
                    break;
            }

            MessageText.Text = MessageText.Text.Insert(MessageText.SelectionStart, tempTag);

            MessageText.SelectionStart = selectionStart + tempTag.Length;
        }

        private void arrowsToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            AddArrowTags(e.ClickedItem.Text);
        }

        /// <summary>
        /// This handles the tag inserts at TextBoxMenu/Control Tags/Icons/Arrows.
        /// They are the up, down, left, and right arrows.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddArrowTags(string text)
        {
            int selectionStart = MessageText.SelectionStart;

            string tempTag = "";

            switch (text)
            {
                case "Up":
                    tempTag = "<Up Arrow>";
                    break;
                case "Down":
                    tempTag = "<Down Arrow>";
                    break;
                case "Left":
                    tempTag = "<Left Arrow>";
                    break;
                case "Right":
                    tempTag = "<Right Arrow>";
                    break;
            }

            MessageText.Text = MessageText.Text.Insert(MessageText.SelectionStart, tempTag);

            MessageText.SelectionStart = selectionStart + tempTag.Length;
        }

        private void waitToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            AddWaitTags(e.ClickedItem.Text);
        }

        /// <summary>
        /// This handles the tag inserts at TextBoxMenu/Control Tags/Wait.
        /// They are the wait, wait + dismiss, wait + dismiss (prompt), and dismiss tags.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddWaitTags(string text)
        {
            int selectionStart = MessageText.SelectionStart;

            string tempTag = "";

            switch (text)
            {
                case "Wait":
                    tempTag = "<Wait:0>";
                    break;
                case "Dismiss":
                    tempTag = "<Wait+Dismiss:0>";
                    break;
                case "Wait + Dismiss":
                    tempTag = "<Wait+Dismiss(Prompt)0>";
                    break;
                case "Wait + Dismiss (Prompt)":
                    tempTag = "<Dismiss:0>";
                    break;
            }

            MessageText.Text = MessageText.Text.Insert(MessageText.SelectionStart, tempTag);

            MessageText.SelectionStart = selectionStart + tempTag.Length;
        }

        private void drawToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            AddDrawTags(e.ClickedItem.Text);
        }

        /// <summary>
        /// This handles the tag inserts at TextBoxMenu/Control Tags/Draw.
        /// They are the draw instantly and draw by character tags.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddDrawTags(string text)
        {
            int selectionStart = MessageText.SelectionStart;

            string tempTag = "";

            switch (text)
            {
                case "Instant":
                    tempTag = "<Draw:Instant>";
                    break;
                case "By Char":
                    tempTag = "<Draw:ByChar>";
                    break;
            }

            MessageText.Text = MessageText.Text.Insert(MessageText.SelectionStart, tempTag);

            MessageText.SelectionStart = selectionStart + tempTag.Length;
        }

        #endregion

        #endregion

        #region Text Colors Functions

        private void textColorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorEditor colorEditor = new ColorEditor(ColorList);

            colorEditor.OnColorListSaved += new ColorEditor.SaveColorEdits(colorEditor_saveTheColors);

            colorEditor.Show();
        }

        void colorEditor_saveTheColors(List<ByteColorAlpha> editedColorList)
        {
            for (int i = 0; i <= editedColorList.Count - 1; i++)
            {
                byte[] temp = editedColorList[i].GetBytes();

                ByteColorAlpha copiedColor = new ByteColorAlpha(temp);

                ColorList[i] = copiedColor;
            }
        }
        #endregion

        #region Proofreading Functions

        private void proofreadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProofreadTagsToCodes();
        }

        private void ProofreadTagsToCodes()
        {
            foreach (Message mes in MessageList)
            {
                mes.ProofReadTagstoCodes();
            }

            UpdateSelectedMessage(SelectedMessageIndex);

            MessageBox.Show("There were no control tag errors.");
        }
        #endregion

        #region Searching Functions

        private void Search()
        {
            if (IsSearchByText)
                TextSearch(searchBoxBackPanel.Controls[0].Text);
            else
                IDSearch(Convert.ToInt32(searchBoxBackPanel.Controls[0].Text));
        }

        private void TextSearch(string searchString)
        {
            if (searchString == "")
            {
                MessageBox.Show("Please enter text to search for.");

                return;
            }

            string nonCasedTerm = searchString.ToLower();

            int startingMessageIndex = SelectedMessageIndex + 1;

            if (LastSearchTerm != nonCasedTerm)
                startingMessageIndex = 0;

            LastSearchTerm = nonCasedTerm;

            for (int i = startingMessageIndex; i < MessageList.Count; i++)
            {
                string sourceString = new string(Encoding.ASCII.GetChars(MessageList[i].charData.ToArray())).ToLower();

                if (sourceString.Contains(nonCasedTerm))
                {
                    UpdateSelectedMessage(i);

                    return;
                }
            }

            MessageBox.Show("String not found.");
        }

        private void IDSearch(int searchID)
        {
            for (int i = 0; i < MessageList.Count; i++)
            {
                if (MessageList[i].msgID == searchID)
                {
                    UpdateSelectedMessage(i);

                    return;
                }
            }

            MessageBox.Show("ID not found.");
        }

        private void idOpt_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb != null)
            {
                if (rb.Name == "textOpt")
                {
                    IsSearchByText = true;
                    AddSearchTextBox();
                }

                else
                {
                    IsSearchByText = false;
                    AddIDBox();
                }
            }
        }

        private void AddSearchTextBox()
        {
            searchBoxBackPanel.Controls.Clear();

            TextBox tempSearchText = new TextBox();

            tempSearchText.KeyDown += new KeyEventHandler(updown_KeyDown);

            tempSearchText.Text = LastSearchTerm;

            searchBoxBackPanel.Controls.Add(tempSearchText);
        }

        private void AddIDBox()
        {
            searchBoxBackPanel.Controls.Clear();

            NumericUpDown updown = new NumericUpDown();

            updown.Maximum = 999999;

            updown.Width = 110;

            updown.KeyDown += new KeyEventHandler(updown_KeyDown);

            searchBoxBackPanel.Controls.Add(updown);
        }

        void updown_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchButton.PerformClick();
            }
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            Search();
        }

        #endregion
    }
}

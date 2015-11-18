using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using GameFormatReader.Common;
using TextEditor2.Forms;

namespace TextEditor2
{
    public class Message
    {
        #region Variables from file

        public int charOffset; //Offset of the char data within DAT1

        public ushort msgID; //ID used to refer to the message
        public ushort unknwn1; //Mostly 0, very rarely 0xA or 0x14 or 0x1E
        public ushort unknwn2; //Mostly 0, rarely ranges between 0x259 and 0x265
        public ushort msgType; //What kind of message this is; 0x60 is "normal"

        public byte boxType; //Style of the textbox that text is displayed in
        public byte drawType; //Initial text draw type; modified by control codes
        public byte boxPos; //Position of the textbox
        public byte itemID; //ID of item image if displayed
        public byte unknwn3; //Usually 1 when itemID != 0xFF, but not always
        public byte unknwn4; //Mostly 0, non-0 values vary widely
        public byte unknwn5; //Mostly 0, non-0 values vary widely
        public byte unknwn6; //Mostly 0, non-0 values vary widely
        public byte unknwn7; //Mostly 0, no variation seen

        public ushort maxLines; //Max number of lines allowed in one textbox

        public byte pad; //Padding to nearest 2

        public List<byte> charData; //Actual char data

        #endregion

        public uint originalCharLength;

        public Message()
        {
            charOffset = 0;
            msgID = 0;
            unknwn1 = 0;
            unknwn2 = 0;
            msgType = 0;
            boxType = 0;
            drawType = 0;
            boxPos = 0;
            itemID = 0;
            unknwn3 = 0;
            unknwn4 = 0;
            unknwn5 = 0;
            unknwn6 = 0;
            unknwn7 = 0;
            maxLines = 1;
            pad = 0;

            charData = new List<byte>();
        }

        public Message(GameFormatReader.Common.EndianBinaryReader stream)
        {
            charOffset = stream.ReadInt32();
            msgID = (ushort)stream.ReadInt16();
            unknwn1 = (ushort)stream.ReadInt16();
            unknwn2 = (ushort)stream.ReadInt16();
            msgType = (ushort)stream.ReadInt16();
            boxType = stream.ReadByte();
            drawType = stream.ReadByte();
            boxPos = stream.ReadByte();
            itemID = stream.ReadByte();
            unknwn3 = stream.ReadByte();
            unknwn4 = stream.ReadByte();
            unknwn5 = stream.ReadByte();
            unknwn6 = stream.ReadByte();
            unknwn7 = stream.ReadByte();
            maxLines = (ushort)stream.ReadInt16();
            pad = stream.ReadByte();

            charData = new List<byte>();
        }

        public void GetCharData(GameFormatReader.Common.EndianBinaryReader stream, uint charDataSize)
        {
            charData = new List<byte>(stream.ReadBytes((int)charDataSize));

            originalCharLength = (uint)charData.Count;

            ControlCodesToTags();
        }

        public void ControlCodesToTags()
        {
            //Convert control codes into control tags (binary to "<>")

            for (int i = 0; i < charData.Count; i++)
            {
                //Control codes are signaled by a value of 0x1A
                //If this isn't triggered, we just keep checking
                //the other chars

                if ((byte)charData[i] != 0x1A)
                {
                    continue;
                }

                //Otherwise, we get into the meat of code parsing
                //Call the proper function for the code size

                //We're going to use i as our reference point
                //for the start of the code, for both getting
                //the actual type and deleting it once we insert
                //the control tag.

                switch ((byte)charData[i + 1])
                {
                    case (byte)ControlCodeSizes.FiveBytes:
                        ConvertFiveByteControlCodeToTag(i);
                        break;

                    case (byte)ControlCodeSizes.SixBytes:
                        ConvertSixByteControlCodeToTag(i);
                        break;

                    case (byte)ControlCodeSizes.SevenBytes:
                        ConvertSevenByteControlCodeToTag(i);
                        break;
                }
            }
        }

        public void ControlTagsToCodes()
        {
            ProofReadTagstoCodes();

            for (int i = 0; i < charData.Count; i++)
            {
                if (charData[i] != '<')
                {
                    continue;
                }

                if (charData[i] == '>')
                {
                    //Error correction box call will go here
                }

                List<byte> tagBuffer = new List<byte>();

                uint tagSize = 1;

                while (charData[(int)(i + tagSize)] != '>')
                {
                    tagBuffer.Add(charData[(int)(i + tagSize)]);

                    tagSize += 1;
                }

                //tagSize at this point only covers the text within the angled brackets.
                //This figures the brackets into the size
                tagSize += 1;

                string tempTag = new string(Encoding.ASCII.GetChars(tagBuffer.ToArray()));

                string[] tagArgs = tempTag.Split(':');

                tagArgs[0] = tagArgs[0].ToLower();

                if (tagArgs.Length > 1)
                {
                    tagArgs[1] = tagArgs[1].ToLower();
                }

                List<byte> code = new List<byte>();

                switch (tagArgs[0])
                {
                    #region Five-Byte Codes
                    case "player":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.PlayerName, 0);
                        break;

                    case "draw":
                        if (tagArgs.Length > 1)
                        {
                            if (tagArgs[1] == "instant")
                            {
                                code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.CharDrawInstant, 0);
                            }

                            if (tagArgs[1] == "bychar")
                            {
                                code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.CharDrawByChar, 0);
                            }

                            else
                            {
                                //Error handler
                            }
                        }
                        break;

                    case "two choices":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.TwoChoices, 0);
                        break;

                    case "three choices":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.ThreeChoices, 0);
                        break;

                    case "a button":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.AButtonIcon, 0);
                        break;

                    case "b button":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.BButtonIcon, 0);
                        break;

                    case "c-stick":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.CStickIcon, 0);
                        break;

                    case "l trigger":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.LTriggerIcon, 0);
                        break;

                    case "r trigger":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.RTriggerIcon, 0);
                        break;

                    case "x button":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.XButtonIcon, 0);
                        break;

                    case "y button":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.YButtonIcon, 0);
                        break;

                    case "z button":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.ZButtonIcon, 0);
                        break;

                    case "d pad":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.DPadIcon, 0);
                        break;

                    case "control stick":
                        if (tagArgs.Length == 1)
                        {
                            code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.StaticControlStickIcon, 0);
                        }

                        else
                        {
                            switch (tagArgs[1])
                            {
                                #region Direction Switch
                                case "up":
                                    code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.ControlStickMovingUp, 0);
                                break;

                                case "down":
                                code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.ControlStickMovingDown, 0);
                                break;

                                case "left":
                                code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.ControlStickMovingLeft, 0);
                                break;

                                case "right":
                                code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.ControlStickMovingRight, 0);
                                break;

                                case "up+down":
                                code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.ControlStickMovingUpAndDown, 0);
                                break;

                                case "left+right":
                                code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.ControlStickMovingLeftAndRight, 0);
                                break;

                                default:
                                    //Error handling
                                break;
                                #endregion
                            }
                        }
                        break;

                    case "left arrow":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.LeftArrowIcon, 0);
                        break;

                    case "right arrow":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.RightArrowIcon, 0);
                        break;

                    case "up arrow":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.UpArrowIcon, 0);
                        break;

                    case "down arrow":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.DownArrowIcon, 0);
                        break;

                    case "choice one":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.ChoiceOne, 0);
                        break;

                    case "choice two":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.ChoiceTwo, 0);
                        break;

                    case "canon game balls":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.CanonGameBalls, 0);
                        break;

                    case "broken vase payment":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.BrokenVasePayment, 0);
                        break;

                    case "auction attendee":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.AuctionCharacter, 0);
                        break;

                    case "auction item name":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.AuctionItemName, 0);
                        break;

                    case "auction attendee bid":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.AuctionPersonBid, 0);
                        break;

                    case "auction starting bid":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.AuctionStartingBid, 0);
                        break;

                    case "player auction bid selector":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.PlayerAuctionBidSelector, 0);
                        break;

                    case "starburst a button":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.StarburstAIcon, 0);
                        break;

                    case "orca blow count":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.OrcaBlowCount, 0);
                        break;

                    case "pirate ship password":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.PirateShipPassword, 0);
                        break;

                    case "target starburst":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.TargetStarburstIcon, 0);
                        break;

                    case "player letter count":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.PostOfficeGamePlayerLetterCount, 0);
                        break;

                    case "letter rupee reward":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.PostOfficeGameRupeeReward, 0);
                        break;

                    case "letters":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.PostBoxLetterCount, 0);
                        break;

                    case "remaining koroks":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.RemainingKoroks, 0);
                        break;

                    case "remaining forest water time":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.RemainingForestWaterTime, 0);
                        break;

                    case "flight control game time":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.FlightPlatformGameTime, 0);
                        break;

                    case "flight control game record":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.FlightPlatformGameRecord, 0);
                        break;

                    case "beedle points":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.BeedlePointCount, 0);
                        break;

                    case "joy pendant count (ms. marie)":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.JoyPendantCountMsMarie, 0);
                        break;

                    case "pendant total":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.MsMariePendantTotal, 0);
                        break;

                    case "pig game time":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.PigGameTime, 0);
                        break;

                    case "sailing game rupee reward":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.SailingGameRupeeReward, 0);
                        break;

                    case "current bomb capacity":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.CurrentBombCapacity, 0);
                        break;

                    case "current arrow capacity":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.CurrentArrowCapacity, 0);
                        break;

                    case "heart icon":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.HeartIcon, 0);
                        break;

                    case "music note icon":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.MusicNoteIcon, 0);
                        break;

                    case "target letter count":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.TargetLetterCount, 0);
                        break;

                    case "fishman hit count":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.FishmanHitCount, 0);
                        break;

                    case "fishman rupee reward":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.FishmanRupeeReward, 0);
                        break;

                    case "boko baba seed count":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.BokoBabaSeedCount, 0);
                        break;

                    case "skull necklace count":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.SkullNecklaceCount, 0);
                        break;

                    case "chu jelly count":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.ChuJellyCount, 0);
                        break;

                    case "joy pendant count (beedle)":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.JoyPendantCountBeedle, 0);
                        break;

                    case "golden feather count":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.GoldenFeatherCount, 0);
                        break;

                    case "knight's crest count":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.KnightsCrestCount, 0);
                        break;

                    case "beedle price offer":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.BeedlePriceOffer, 0);
                        break;

                    case "boko baba seed sell selector":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.BokoBabaSeedSellSelector, 0);
                        break;

                    case "skull necklace sell selector":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.SkullNecklaceSellSelector, 0);
                        break;

                    case "chu jelly sell selector":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.ChuJellySellSelector, 0);
                        break;

                    case "joy pendant sell selector":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.JoyPendantSellSelector, 0);
                        break;

                    case "golden feather sell selector":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.GoldenFeatherSellSelector, 0);
                        break;

                    case "knight's crest sell selector":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.KnightsCrestSellSelector, 0);
                        break;

                    case "sound":
                        if (tagArgs.Length > 1)
                        {
                            code = ConvertTagToFiveByteControlCode(i, 1, 1, Convert.ToInt16(tagArgs[1]));
                        }

                        else
                        {
                            //Error handler
                        }
                        break;

                    case "camera modifier":
                        if (tagArgs.Length > 1)
                        {
                            code = ConvertTagToFiveByteControlCode(i, 2, 2, Convert.ToInt16(tagArgs[1]));
                        }

                        else
                        {
                            //Error handler
                        }
                        break;

                    case "anim":
                        if (tagArgs.Length > 1)
                        {
                            code = ConvertTagToFiveByteControlCode(i, 3, 3, Convert.ToInt16(tagArgs[1]));
                        }
                        
                        else
                        {
                            //Error handler
                        }
                        break;
                    #endregion

                    #region Six-Byte Code
                    case "color":
                        if (tagArgs.Length > 1)
                        {
                            code.Add(0x1A);
                            code.Add(0x06);
                            code.Add(0xFF);
                            code.Add(0x00);
                            code.Add(0x00);
                            code.Add(Convert.ToByte(tagArgs[1]));
                        }

                        else
                        {
                            //error handling
                        }
                        break;
                    #endregion

                    #region Seven-Byte Codes
                    case "scale":
                            if (tagArgs.Length > 1)
                        {
                            code.Add(0x1A);
                            code.Add(0x07);
                            code.Add(0xFF);
                            code.Add(0x00);
                            code.Add((byte)SevenByteTypes.SetTextSize);

                            byte[] tempShort = BitConverter.GetBytes(Convert.ToInt16(tagArgs[1]));

                            code.Add(tempShort[1]);
                            code.Add(tempShort[0]);
                        }

                        else
                        {
                            //error handling
                        }
                        break;

                    case "wait + dismiss (prompt)":
                        if (tagArgs.Length > 1)
                        {
                            code.Add(0x1A);
                            code.Add(0x07);
                            code.Add(0x00);
                            code.Add(0x00);
                            code.Add((byte)SevenByteTypes.WaitAndDismissWithPrompt);

                            byte[] tempShort = BitConverter.GetBytes(Convert.ToInt16(tagArgs[1]));

                            code.Add(tempShort[1]);
                            code.Add(tempShort[0]);
                        }

                        else
                        {
                            //error handling
                        }
                        break;

                    case "wait + dismiss":
                        if (tagArgs.Length > 1)
                        {
                            code.Add(0x1A);
                            code.Add(0x07);
                            code.Add(0x00);
                            code.Add(0x00);
                            code.Add((byte)SevenByteTypes.WaitAndDismiss);

                            byte[] tempShort = BitConverter.GetBytes(Convert.ToInt16(tagArgs[1]));

                            code.Add(tempShort[1]);
                            code.Add(tempShort[0]);
                        }

                        else
                        {
                            //error handling
                        }
                        break;

                    case "dismiss":
                        if (tagArgs.Length > 1)
                        {
                            code.Add(0x1A);
                            code.Add(0x07);
                            code.Add(0x00);
                            code.Add(0x00);
                            code.Add((byte)SevenByteTypes.Dismiss);

                            byte[] tempShort = BitConverter.GetBytes(Convert.ToInt16(tagArgs[1]));

                            code.Add(tempShort[1]);
                            code.Add(tempShort[0]);
                        }

                        else
                        {
                            //error handling
                        }
                        break;

                    case "dummy":
                        if (tagArgs.Length > 1)
                        {
                            code.Add(0x1A);
                            code.Add(0x07);
                            code.Add(0x00);
                            code.Add(0x00);
                            code.Add((byte)SevenByteTypes.Dummy);

                            byte[] tempShort = BitConverter.GetBytes(Convert.ToInt16(tagArgs[1]));

                            code.Add(tempShort[1]);
                            code.Add(tempShort[0]);
                        }

                        else
                        {
                            //error handling
                        }
                        break;

                    case "wait":
                        if (tagArgs.Length > 1)
                        {
                            code.Add(0x1A);
                            code.Add(0x07);
                            code.Add(0x00);
                            code.Add(0x00);
                            code.Add((byte)SevenByteTypes.Wait);

                            byte[] tempShort = BitConverter.GetBytes(Convert.ToInt16(tagArgs[1]));

                            code.Add(tempShort[1]);
                            code.Add(tempShort[0]);
                        }

                        else
                        {
                            //error handling
                        }
                        break;
                    #endregion
                }

                charData.InsertRange((int)(i + tagSize), code.ToArray());
                
                charData.RemoveRange(i, (int)tagSize);

                i += code.Count - 1;
            }
        }

        public void WriteEntryData(GameFormatReader.Common.EndianBinaryWriter stream)
        {
            //Write entry data to stream
            stream.Write((int)charOffset);

            stream.Write((short)msgID);

            stream.Write((short)unknwn1);

            stream.Write((short)unknwn2);

            stream.Write((short)msgType);

            stream.Write((byte)boxType);

            stream.Write((byte)drawType);

            stream.Write((byte)boxPos);

            stream.Write((byte)itemID);

            stream.Write((byte)unknwn3);

            stream.Write((byte)unknwn4);

            stream.Write((byte)unknwn5);

            stream.Write((byte)unknwn6);

            stream.Write((byte)unknwn7);

            stream.Write((short)maxLines);

            stream.Write((byte)pad);
        }

        public void WriteCharData(GameFormatReader.Common.EndianBinaryWriter stream)
        {
            stream.Write(charData.ToArray());

            if (charData[charData.Count - 1] != (char)0)
            {
                stream.Write((byte)0);
            }
        }

        public void ConvertFiveByteControlCodeToTag(int startIndex)
        {
            short variable = 0;

            string tagType = "";

            string completeTag = "";

            if ((byte)charData[startIndex + 2] == 0)
            {

                #region Type switch

                switch ((byte)charData[startIndex + 4])
                {
                    case (byte)FiveByteTypes.PlayerName:
                        tagType = "Player";
                        break;
                    case (byte)FiveByteTypes.CharDrawInstant:
                        tagType = "Draw:Instant";
                        break;
                    case (byte)FiveByteTypes.CharDrawByChar:
                        tagType = "Draw:ByChar";
                        break;
                    case (byte)FiveByteTypes.TwoChoices:
                        tagType = "Two Choices";
                        break;
                    case (byte)FiveByteTypes.ThreeChoices:
                        tagType = "Three Choices";
                        break;
                    case (byte)FiveByteTypes.AButtonIcon:
                        tagType = "A Button";
                        break;
                    case (byte)FiveByteTypes.BButtonIcon:
                        tagType = "B Button";
                        break;
                    case (byte)FiveByteTypes.CStickIcon:
                        tagType = "C-Stick";
                        break;
                    case (byte)FiveByteTypes.LTriggerIcon:
                        tagType = "L Trigger";
                        break;
                    case (byte)FiveByteTypes.RTriggerIcon:
                        tagType = "R Trigger";
                        break;
                    case (byte)FiveByteTypes.XButtonIcon:
                        tagType = "X Button";
                        break;
                    case (byte)FiveByteTypes.YButtonIcon:
                        tagType = "Y Button";
                        break;
                    case (byte)FiveByteTypes.ZButtonIcon:
                        tagType = "Z Button";
                        break;
                    case (byte)FiveByteTypes.DPadIcon:
                        tagType = "D Pad";
                        break;
                    case (byte)FiveByteTypes.StaticControlStickIcon:
                        tagType = "Control Stick";
                        break;
                    case (byte)FiveByteTypes.LeftArrowIcon:
                        tagType = "Left Arrow";
                        break;
                    case (byte)FiveByteTypes.RightArrowIcon:
                        tagType = "Right Arrow";
                        break;
                    case (byte)FiveByteTypes.UpArrowIcon:
                        tagType = "Up Arrow";
                        break;
                    case (byte)FiveByteTypes.DownArrowIcon:
                        tagType = "Down Arrow";
                        break;
                    case (byte)FiveByteTypes.ControlStickMovingUp:
                        tagType = "Control Stick:Up";
                        break;
                    case (byte)FiveByteTypes.ControlStickMovingDown:
                        tagType = "Control Stick:Down";
                        break;
                    case (byte)FiveByteTypes.ControlStickMovingLeft:
                        tagType = "Control Stick:Left";
                        break;
                    case (byte)FiveByteTypes.ControlStickMovingRight:
                        tagType = "Control Stick:Right";
                        break;
                    case (byte)FiveByteTypes.ControlStickMovingUpAndDown:
                        tagType = "Control Stick:Up+Down";
                        break;
                    case (byte)FiveByteTypes.ControlStickMovingLeftAndRight:
                        tagType = "Control Stick:Left+Right";
                        break;
                    case (byte)FiveByteTypes.ChoiceOne:
                        tagType = "Choice One";
                        break;
                    case (byte)FiveByteTypes.ChoiceTwo:
                        tagType = "Choice Two";
                        break;
                    case (byte)FiveByteTypes.CanonGameBalls:
                        tagType = "Canon Game Balls";
                        break;
                    case (byte)FiveByteTypes.BrokenVasePayment:
                        tagType = "Broken Vase Payment";
                        break;
                    case (byte)FiveByteTypes.AuctionCharacter:
                        tagType = "Auction Attendee";
                        break;
                    case (byte)FiveByteTypes.AuctionItemName:
                        tagType = "Auction Item Name";
                        break;
                    case (byte)FiveByteTypes.AuctionPersonBid:
                        tagType = "Auction Attendee Bid";
                        break;
                    case (byte)FiveByteTypes.AuctionStartingBid:
                        tagType = "Auction Starting Bid";
                        break;
                    case (byte)FiveByteTypes.PlayerAuctionBidSelector:
                        tagType = "Player Auction Bid Selector";
                        break;
                    case (byte)FiveByteTypes.StarburstAIcon:
                        tagType = "Starburst A Button";
                        break;
                    case (byte)FiveByteTypes.OrcaBlowCount:
                        tagType = "Orca Blow Count";
                        break;
                    case (byte)FiveByteTypes.PirateShipPassword:
                        tagType = "Pirate Ship Password";
                        break;
                    case (byte)FiveByteTypes.TargetStarburstIcon:
                        tagType = "Target Starburst";
                        break;
                    case (byte)FiveByteTypes.PostOfficeGamePlayerLetterCount:
                        tagType = "Player Letter Count";
                        break;
                    case (byte)FiveByteTypes.PostOfficeGameRupeeReward:
                        tagType = "Letter Rupee Reward";
                        break;
                    case (byte)FiveByteTypes.PostBoxLetterCount:
                        tagType = "Letters";
                        break;
                    case (byte)FiveByteTypes.RemainingKoroks:
                        tagType = "Remaining Koroks";
                        break;
                    case (byte)FiveByteTypes.RemainingForestWaterTime:
                        tagType = "Remaining Forest Water Time";
                        break;
                    case (byte)FiveByteTypes.FlightPlatformGameTime:
                        tagType = "Flight Control Game Time";
                        break;
                    case (byte)FiveByteTypes.FlightPlatformGameRecord:
                        tagType = "Flight Control Game Record";
                        break;
                    case (byte)FiveByteTypes.BeedlePointCount:
                        tagType = "Beedle Points";
                        break;
                    case (byte)FiveByteTypes.JoyPendantCountMsMarie:
                        tagType = "Joy Pendant Count (Ms. Marie)";
                        break;
                    case (byte)FiveByteTypes.MsMariePendantTotal:
                        tagType = "Pendant Total";
                        break;
                    case (byte)FiveByteTypes.PigGameTime:
                        tagType = "Pig Game Time";
                        break;
                    case (byte)FiveByteTypes.SailingGameRupeeReward:
                        tagType = "Sailing Game Rupee Reward";
                        break;
                    case (byte)FiveByteTypes.CurrentBombCapacity:
                        tagType = "Current Bomb Capacity";
                        break;
                    case (byte)FiveByteTypes.CurrentArrowCapacity:
                        tagType = "Current Arrow Capacity";
                        break;
                    case (byte)FiveByteTypes.HeartIcon:
                        tagType = "Heart Icon";
                        break;
                    case (byte)FiveByteTypes.MusicNoteIcon:
                        tagType = "Music Note Icon";
                        break;
                    case (byte)FiveByteTypes.TargetLetterCount:
                        tagType = "Target Letter Count";
                        break;
                    case (byte)FiveByteTypes.FishmanHitCount:
                        tagType = "Fishman Hit Count";
                        break;
                    case (byte)FiveByteTypes.FishmanRupeeReward:
                        tagType = "Fishman Rupee Reward";
                        break;
                    case (byte)FiveByteTypes.BokoBabaSeedCount:
                        tagType = "Boko Baba Seed Count";
                        break;
                    case (byte)FiveByteTypes.SkullNecklaceCount:
                        tagType = "Skull Necklace Count";
                        break;
                    case(byte)FiveByteTypes.ChuJellyCount:
                        tagType = "Chu Jelly Count";
                        break;
                    case (byte)FiveByteTypes.JoyPendantCountBeedle:
                        tagType = "Joy Pendant Count (Beedle)";
                        break;
                    case (byte)FiveByteTypes.GoldenFeatherCount:
                        tagType = "Golden Feather Count";
                        break;
                    case (byte)FiveByteTypes.KnightsCrestCount:
                        tagType = "Knight's Crest Count";
                        break;
                    case (byte)FiveByteTypes.BeedlePriceOffer:
                        tagType = "Beedle Price Offer";
                        break;
                    case (byte)FiveByteTypes.BokoBabaSeedSellSelector:
                        tagType = "Boko Baba Seed Sell Selector";
                        break;
                    case (byte)FiveByteTypes.SkullNecklaceSellSelector:
                        tagType = "Skull Necklace Sell Selector";
                        break;
                    case (byte)FiveByteTypes.ChuJellySellSelector:
                        tagType = "Chu Jelly Sell Selector";
                        break;
                    case (byte)FiveByteTypes.JoyPendantSellSelector:
                        tagType = "Joy Pendant Sell Selector";
                        break;
                    case (byte)FiveByteTypes.GoldenFeatherSellSelector:
                        tagType = "Golden Feather Sell Selector";
                        break;
                    case (byte)FiveByteTypes.KnightsCrestSellSelector:
                        tagType = "Knight's Crest Sell Selector";
                        break;
                    default:
                        tagType = "Unknown:" + (byte)charData[startIndex + 4];
                        break;
                }

                #endregion

                completeTag = "<" + tagType + ">";

                charData.InsertRange(startIndex + 5, Encoding.ASCII.GetBytes(completeTag));

                charData.RemoveRange(startIndex, 5);
            }

            else if (((byte)charData[startIndex + 2] == 1))
            {
                tagType = "Sound";

                byte[] byteBuffer = new byte[] { (byte)charData[startIndex + 4], (byte)charData[startIndex + 3] };

                variable = BitConverter.ToInt16(byteBuffer, 0);

                completeTag = "<" + tagType + ":" + variable + ">";

                charData.InsertRange(startIndex + 5, Encoding.ASCII.GetBytes(completeTag));

                charData.RemoveRange(startIndex, 5);
            }

            else if (((byte)charData[startIndex + 2] == 2))
            {
                tagType = "Camera Modifier";

                byte[] byteBuffer = new byte[] { (byte)charData[startIndex + 4], (byte)charData[startIndex + 3] };

                variable = BitConverter.ToInt16(byteBuffer, 0);

                completeTag = "<" + tagType + ":" + variable + ">";

                charData.InsertRange(startIndex + 5, Encoding.ASCII.GetBytes(completeTag));

                charData.RemoveRange(startIndex, 5);
            }

            else if (((byte)charData[startIndex + 2] == 3))
            {
                tagType = "Anim";

                byte[] byteBuffer = new byte[] { (byte)charData[startIndex + 4], (byte)charData[startIndex + 3] };

                variable = BitConverter.ToInt16(byteBuffer, 0);

                completeTag = "<" + tagType + ":" + variable + ">";

                charData.InsertRange(startIndex + 5, Encoding.ASCII.GetBytes(completeTag));

                charData.RemoveRange(startIndex, 5);
            }
        }

        public void ConvertSixByteControlCodeToTag(int startIndex)
        {
            //Convert a SixByte control code to a tag

            //Since there is only one SixByte control code, we only
            //need this. Yay!

            //Stores the color index
            ushort variable = 0;

            //Tag type for combination
            string tagType = "Color";

            //Empty complete tag
            string completeTag = "";

            //Create and fill buffer to prepare for short conversion
            byte[] buffer = new byte[] { (byte)charData[startIndex + 5], (byte)charData[startIndex + 4] };

            ///Convert two bytes to short
            variable = (ushort)BitConverter.ToInt16(buffer, 0);

            //Build complete tag
            completeTag = "<" + tagType + ":" + variable + ">";

            //Insert tag after the original code
            charData.InsertRange(startIndex + 6, Encoding.ASCII.GetBytes(completeTag));
            //Delete original code
            charData.RemoveRange(startIndex, 6);
        }

        public void ConvertSevenByteControlCodeToTag(int startIndex)
        {
            //Convert a SevenByte control code to a tag

            //Stores variable
            uint variable = 0;

            //Stores tag type
            string tagType = "";

            //Empty complete tag
            string completeTag = "";

            //Get tagType by comparing the type with
            //values in enum SevenByteTypes
            switch ((byte)charData[startIndex + 4])
            {
                case (byte)SevenByteTypes.SetTextSize:
                    tagType = "Scale";
                    break;
                case (byte)SevenByteTypes.WaitAndDismissWithPrompt:
                    tagType = "Wait + Dismiss (prompt)";
                    break;
                case (byte)SevenByteTypes.WaitAndDismiss:
                    tagType = "Wait + Dismiss";
                    break;
                case (byte)SevenByteTypes.Dismiss:
                    tagType = "Dismiss";
                    break;
                case (byte)SevenByteTypes.Dummy:
                    tagType = "Dummy";
                    break;
                case (byte)SevenByteTypes.Wait:
                    tagType = "Wait";
                    break;
                default:
                    tagType = "Unknown " + (byte)charData[startIndex + 4];
                    break;
            }

            //Create and fill buffer to prepare for short conversion
            byte[] buffer = new byte[] { (byte)charData[startIndex + 6], (byte)charData[startIndex + 5] };

            ///Convert two bytes to short
            variable = (ushort)BitConverter.ToInt16(buffer, 0);

            //Build complete tag
            completeTag = "<" + tagType + ":" + variable + ">";

            //Insert tag after the original code
            charData.InsertRange(startIndex + 7, Encoding.ASCII.GetBytes(completeTag));

            //Delete original code
            charData.RemoveRange(startIndex, 7);
        }

        public List<byte> ConvertTagToFiveByteControlCode(int startIndex, byte field2Type, byte normalType, short arg)
        {
            List<byte> code = new List<byte>();

            byte[] temp;

            code.Add(0x1A);
            code.Add(0x05);

            switch (field2Type)
            {
                case 0:
                    code.Add(0x00);
                    code.Add(0x00);
                    code.Add(normalType);
                    break;
                case 1:
                    code.Add(field2Type);

                    temp = BitConverter.GetBytes(Convert.ToInt16(arg));

                    code.Add(temp[1]);
                    code.Add(temp[0]);
                    break;
                case 2:
                    code.Add(field2Type);

                    temp = BitConverter.GetBytes(Convert.ToInt16(arg));

                    code.Add(temp[1]);
                    code.Add(temp[0]);
                    break;
                case 3:
                    code.Add(field2Type);

                    temp = BitConverter.GetBytes(Convert.ToInt16(arg));

                    code.Add(temp[1]);
                    code.Add(temp[0]);
                    break;
            }

            return code;
        }

        public Message Copy()
        {
            Message copyMes = new Message();

            copyMes.charOffset = charOffset;
            copyMes.msgID = msgID;
            copyMes.unknwn1 = unknwn1;
            copyMes.unknwn2 = unknwn2;
            copyMes.msgType = msgType;
            copyMes.boxType = boxType;
            copyMes.drawType = drawType;
            copyMes.boxPos = boxPos;
            copyMes.itemID = itemID;
            copyMes.unknwn3 = unknwn3;
            copyMes.unknwn4 = unknwn4;
            copyMes.unknwn5 = unknwn5;
            copyMes.unknwn6 = unknwn6;
            copyMes.unknwn7 = unknwn7;
            copyMes.maxLines = maxLines;
            copyMes.pad = pad;

            copyMes.originalCharLength = originalCharLength;

            copyMes.charData = new List<byte>(charData);

            return copyMes;
        }

        public void ProofReadTagstoCodes()
        {
            List<byte> testList = new List<byte>(charData);

            for (int i = 0; i < testList.Count; i++)
            {
                if ((testList[i] != '<') && (testList[i] != '>'))
                {
                    continue;
                }

                if (testList[i] == '>')
                {
                    TagErrorHandler errorHandler = new TagErrorHandler("Unexpected Tag End", "An uexpected '>' bracket was found. Is there a" + Environment.NewLine + "missing '<' bracket?", charData);

                    DialogResult result = errorHandler.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        //MessageBox.Show("works");

                        charData = errorHandler.GetCorrectedData();

                        ProofReadTagstoCodes();

                        return;
                    }
                }

                List<byte> tagBuffer = new List<byte>();

                uint tagSize = 1;

                while (testList[(int)(i + tagSize)] != '>')
                {
                    tagBuffer.Add(testList[(int)(i + tagSize)]);

                    tagSize += 1;

                    if (i + tagSize >= testList.Count)
                    {
                        TagErrorHandler errorHandler = new TagErrorHandler("Endless Tag", "A '<' bracket was detected without a closing '>' bracket." + Environment.NewLine + "Please add a '>' bracket or delete the '<' bracket.", charData);

                        DialogResult result = errorHandler.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            //MessageBox.Show("works");

                            charData = errorHandler.GetCorrectedData();

                            ProofReadTagstoCodes();

                            return;
                        }
                    }
                }

                //tagSize at this point only covers the text within the angled brackets.
                //This figures the brackets into the size
                tagSize += 1;

                string tempTag = new string(Encoding.ASCII.GetChars(tagBuffer.ToArray()));

                string[] tagArgs = tempTag.Split(':');

                tagArgs[0] = tagArgs[0].ToLower();

                if (tagArgs.Length > 1)
                {
                    tagArgs[1] = tagArgs[1].ToLower();
                }

                List<byte> code = new List<byte>();

                switch (tagArgs[0])
                {
                    #region Five-Byte Codes
                    case "player":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.PlayerName, 0);
                        break;

                    case "draw":
                        if (tagArgs.Length > 1)
                        {
                            if (tagArgs[1] == "instant")
                            {
                                code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.CharDrawInstant, 0);

                                break;
                            }

                            if (tagArgs[1] == "bychar")
                            {
                                code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.CharDrawByChar, 0);

                                break;
                            }

                            else
                            {
                                TagErrorHandler errorHandler = new TagErrorHandler("Invalid 'Draw' Type", "A 'Draw' tag has an invalid type." + Environment.NewLine + "Please choose either 'Instant' or 'ByChar'.", charData);

                                DialogResult result = errorHandler.ShowDialog();

                                if (result == DialogResult.OK)
                                {
                                    //MessageBox.Show("works");

                                    charData = errorHandler.GetCorrectedData();

                                    ProofReadTagstoCodes();

                                    return;
                                }
                            }
                        }
                        break;

                    case "two choices":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.TwoChoices, 0);
                        break;

                    case "three choices":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.ThreeChoices, 0);
                        break;

                    case "a button":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.AButtonIcon, 0);
                        break;

                    case "b button":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.BButtonIcon, 0);
                        break;

                    case "c-stick":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.CStickIcon, 0);
                        break;

                    case "l trigger":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.LTriggerIcon, 0);
                        break;

                    case "r trigger":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.RTriggerIcon, 0);
                        break;

                    case "x button":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.XButtonIcon, 0);
                        break;

                    case "y button":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.YButtonIcon, 0);
                        break;

                    case "z button":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.ZButtonIcon, 0);
                        break;

                    case "d pad":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.DPadIcon, 0);
                        break;

                    case "control stick":
                        if (tagArgs.Length == 1)
                        {
                            code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.StaticControlStickIcon, 0);
                        }

                        else
                        {
                            switch (tagArgs[1])
                            {
                                #region Direction Switch
                                case "up":
                                    code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.ControlStickMovingUp, 0);
                                    break;

                                case "down":
                                    code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.ControlStickMovingDown, 0);
                                    break;

                                case "left":
                                    code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.ControlStickMovingLeft, 0);
                                    break;

                                case "right":
                                    code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.ControlStickMovingRight, 0);
                                    break;

                                case "up+down":
                                    code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.ControlStickMovingUpAndDown, 0);
                                    break;

                                case "left+right":
                                    code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.ControlStickMovingLeftAndRight, 0);
                                    break;

                                default:
                                    TagErrorHandler errorHandler = new TagErrorHandler("Invalid 'Control Stick' Argument", "A 'Control Stick:' tag has an invalid argument." + Environment.NewLine + "Please choose from 'Up', 'Down', 'Left', 'Right'," + Environment.NewLine + " 'Up+Down', or 'Left+Right'.", charData);
                                    DialogResult result = errorHandler.ShowDialog();
                                    
                                    if (result == DialogResult.OK)
                                    {
                                        //MessageBox.Show("works");
                                        
                                        charData = errorHandler.GetCorrectedData();
                                        
                                        ProofReadTagstoCodes();

                                        return;
                                    }
                                    
                                    return;
                                #endregion
                            }
                        }
                        break;

                    case "left arrow":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.LeftArrowIcon, 0);
                        break;

                    case "right arrow":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.RightArrowIcon, 0);
                        break;

                    case "up arrow":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.UpArrowIcon, 0);
                        break;

                    case "down arrow":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.DownArrowIcon, 0);
                        break;

                    case "choice one":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.ChoiceOne, 0);
                        break;

                    case "choice two":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.ChoiceTwo, 0);
                        break;

                    case "canon game balls":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.CanonGameBalls, 0);
                        break;

                    case "broken vase payment":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.BrokenVasePayment, 0);
                        break;

                    case "auction attendee":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.AuctionCharacter, 0);
                        break;

                    case "auction item name":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.AuctionItemName, 0);
                        break;

                    case "auction attendee bid":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.AuctionPersonBid, 0);
                        break;

                    case "auction starting bid":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.AuctionStartingBid, 0);
                        break;

                    case "player auction bid selector":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.PlayerAuctionBidSelector, 0);
                        break;

                    case "starburst a button":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.StarburstAIcon, 0);
                        break;

                    case "orca blow count":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.OrcaBlowCount, 0);
                        break;

                    case "pirate ship password":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.PirateShipPassword, 0);
                        break;

                    case "target starburst":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.TargetStarburstIcon, 0);
                        break;

                    case "player letter count":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.PostOfficeGamePlayerLetterCount, 0);
                        break;

                    case "letter rupee reward":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.PostOfficeGameRupeeReward, 0);
                        break;

                    case "letters":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.PostBoxLetterCount, 0);
                        break;

                    case "remaining koroks":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.RemainingKoroks, 0);
                        break;

                    case "remaining forest water time":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.RemainingForestWaterTime, 0);
                        break;

                    case "flight control game time":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.FlightPlatformGameTime, 0);
                        break;

                    case "flight control game record":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.FlightPlatformGameRecord, 0);
                        break;

                    case "beedle points":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.BeedlePointCount, 0);
                        break;

                    case "joy pendant count (ms. marie)":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.JoyPendantCountMsMarie, 0);
                        break;

                    case "pendant total":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.MsMariePendantTotal, 0);
                        break;

                    case "pig game time":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.PigGameTime, 0);
                        break;

                    case "sailing game rupee reward":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.SailingGameRupeeReward, 0);
                        break;

                    case "current bomb capacity":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.CurrentBombCapacity, 0);
                        break;

                    case "current arrow capacity":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.CurrentArrowCapacity, 0);
                        break;

                    case "heart icon":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.HeartIcon, 0);
                        break;

                    case "music note icon":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.MusicNoteIcon, 0);
                        break;

                    case "target letter count":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.TargetLetterCount, 0);
                        break;

                    case "fishman hit count":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.FishmanHitCount, 0);
                        break;

                    case "fishman rupee reward":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.FishmanRupeeReward, 0);
                        break;

                    case "boko baba seed count":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.BokoBabaSeedCount, 0);
                        break;

                    case "skull necklace count":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.SkullNecklaceCount, 0);
                        break;

                    case "chu jelly count":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.ChuJellyCount, 0);
                        break;

                    case "joy pendant count (beedle)":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.JoyPendantCountBeedle, 0);
                        break;

                    case "golden feather count":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.GoldenFeatherCount, 0);
                        break;

                    case "knight's crest count":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.KnightsCrestCount, 0);
                        break;

                    case "beedle price offer":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.BeedlePriceOffer, 0);
                        break;

                    case "boko baba seed sell selector":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.BokoBabaSeedSellSelector, 0);
                        break;

                    case "skull necklace sell selector":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.SkullNecklaceSellSelector, 0);
                        break;

                    case "chu jelly sell selector":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.ChuJellySellSelector, 0);
                        break;

                    case "joy pendant sell selector":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.JoyPendantSellSelector, 0);
                        break;

                    case "golden feather sell selector":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.GoldenFeatherSellSelector, 0);
                        break;

                    case "knight's crest sell selector":
                        code = ConvertTagToFiveByteControlCode(i, 0, (byte)FiveByteTypes.KnightsCrestSellSelector, 0);
                        break;

                    case "sound":
                        if (tagArgs.Length > 1)
                        {
                            bool tagSafe = CheckTagNumArgs(tagArgs[1], tagArgs[0]);

                            if (tagSafe == false)
                            {
                                return;
                            }

                            code = ConvertTagToFiveByteControlCode(i, 1, 1, Convert.ToInt16(tagArgs[1]));
                        }

                        else
                        {
                            TagErrorHandler errorHandler = new TagErrorHandler("Missing Argument", "A tag '" + tagArgs[0] + "' requires a numerical argument." + Environment.NewLine + "Please add an argument by following the tag" + Environment.NewLine + "with a ':' and a number.", charData);

                            DialogResult result = errorHandler.ShowDialog();

                            if (result == DialogResult.OK)
                            {
                                //MessageBox.Show("works");

                                charData = errorHandler.GetCorrectedData();

                                ProofReadTagstoCodes();

                                return;
                            }
                        }
                        break;

                    case "camera modifier":
                        if (tagArgs.Length > 1)
                        {
                            bool tagSafe = CheckTagNumArgs(tagArgs[1], tagArgs[0]);

                            if (tagSafe == false)
                            {
                                return;
                            }

                            code = ConvertTagToFiveByteControlCode(i, 2, 2, Convert.ToInt16(tagArgs[1]));
                        }

                        else
                        {
                            TagErrorHandler errorHandler = new TagErrorHandler("Missing Argument", "A tag '" + tagArgs[0] + "' requires a numerical argument." + Environment.NewLine + "Please add an argument by following the tag" + Environment.NewLine + "with a ':' and a number.", charData);

                            DialogResult result = errorHandler.ShowDialog();

                            if (result == DialogResult.OK)
                            {
                                //MessageBox.Show("works");

                                charData = errorHandler.GetCorrectedData();

                                ProofReadTagstoCodes();

                                return;
                            }
                        }
                        break;

                    case "anim":
                        if (tagArgs.Length > 1)
                        {
                            bool tagSafe = CheckTagNumArgs(tagArgs[1], tagArgs[0]);

                            if (tagSafe == false)
                            {
                                return;
                            }

                            code = ConvertTagToFiveByteControlCode(i, 3, 3, Convert.ToInt16(tagArgs[1]));
                        }

                        else
                        {
                            TagErrorHandler errorHandler = new TagErrorHandler("Missing Argument", "A tag '" + tagArgs[0] + "' requires a numerical argument." + Environment.NewLine + "Please add an argument by following the tag" + Environment.NewLine + "with a ':' and a number.", charData);

                            DialogResult result = errorHandler.ShowDialog();

                            if (result == DialogResult.OK)
                            {
                                //MessageBox.Show("works");

                                charData = errorHandler.GetCorrectedData();

                                ProofReadTagstoCodes();

                                return;
                            }
                        }
                        break;
                    #endregion

                    #region Six-Byte Code
                    case "color":
                        if (tagArgs.Length > 1)
                        {
                            bool tagSafe = CheckTagNumArgs(tagArgs[1], tagArgs[0]);

                            if (tagSafe == false)
                            {
                                return;
                            }

                            code.Add(0x1A);
                            code.Add(0x06);
                            code.Add(0xFF);
                            code.Add(0x00);
                            code.Add(0x00);
                            code.Add(Convert.ToByte(tagArgs[1]));
                        }

                        else
                        {
                            TagErrorHandler errorHandler = new TagErrorHandler("Missing Argument", "A tag '" + tagArgs[0] + "' requires a numerical argument." + Environment.NewLine + "Please add an argument by following the tag" + Environment.NewLine + "with a ':' and a number.", charData);

                            DialogResult result = errorHandler.ShowDialog();

                            if (result == DialogResult.OK)
                            {
                                //MessageBox.Show("works");

                                charData = errorHandler.GetCorrectedData();

                                ProofReadTagstoCodes();

                                return;
                            }
                        }
                        break;
                    #endregion

                    #region Seven-Byte Codes
                    case "scale":
                        if (tagArgs.Length > 1)
                        {
                            bool tagSafe = CheckTagNumArgs(tagArgs[1], tagArgs[0]);

                            if (tagSafe == false)
                            {
                                return;
                            }

                            code.Add(0x1A);
                            code.Add(0x07);
                            code.Add(0xFF);
                            code.Add(0x00);
                            code.Add((byte)SevenByteTypes.SetTextSize);

                            byte[] tempShort = BitConverter.GetBytes(Convert.ToInt16(tagArgs[1]));

                            code.Add(tempShort[1]);
                            code.Add(tempShort[0]);
                        }

                        else
                        {
                            TagErrorHandler errorHandler = new TagErrorHandler("Missing Argument", "A tag '" + tagArgs[0] + "' requires a numerical argument." + Environment.NewLine + "Please add an argument by following the tag" + Environment.NewLine + "with a ':' and a number.", charData);

                            DialogResult result = errorHandler.ShowDialog();

                            if (result == DialogResult.OK)
                            {
                                //MessageBox.Show("works");

                                charData = errorHandler.GetCorrectedData();

                                ProofReadTagstoCodes();

                                return;
                            }
                        }
                        break;

                    case "wait + dismiss (prompt)":
                        if (tagArgs.Length > 1)
                        {
                            bool tagSafe = CheckTagNumArgs(tagArgs[1], tagArgs[0]);

                            if (tagSafe == false)
                            {
                                return;
                            }

                            code.Add(0x1A);
                            code.Add(0x07);
                            code.Add(0x00);
                            code.Add(0x00);
                            code.Add((byte)SevenByteTypes.WaitAndDismissWithPrompt);

                            byte[] tempShort = BitConverter.GetBytes(Convert.ToInt16(tagArgs[1]));

                            code.Add(tempShort[1]);
                            code.Add(tempShort[0]);
                        }

                        else
                        {
                            TagErrorHandler errorHandler = new TagErrorHandler("Missing Argument", "A tag '" + tagArgs[0] + "' requires a numerical argument." + Environment.NewLine + "Please add an argument by following the tag" + Environment.NewLine + "with a ':' and a number.", charData);

                            DialogResult result = errorHandler.ShowDialog();

                            if (result == DialogResult.OK)
                            {
                                //MessageBox.Show("works");

                                charData = errorHandler.GetCorrectedData();

                                ProofReadTagstoCodes();

                                return;
                            }
                        }
                        break;

                    case "wait + dismiss":
                        if (tagArgs.Length > 1)
                        {
                            bool tagSafe = CheckTagNumArgs(tagArgs[1], tagArgs[0]);

                            if (tagSafe == false)
                            {
                                return;
                            }

                            code.Add(0x1A);
                            code.Add(0x07);
                            code.Add(0x00);
                            code.Add(0x00);
                            code.Add((byte)SevenByteTypes.WaitAndDismiss);

                            byte[] tempShort = BitConverter.GetBytes(Convert.ToInt16(tagArgs[1]));

                            code.Add(tempShort[1]);
                            code.Add(tempShort[0]);
                        }

                        else
                        {
                            TagErrorHandler errorHandler = new TagErrorHandler("Missing Argument", "A tag '" + tagArgs[0] + "' requires a numerical argument." + Environment.NewLine + "Please add an argument by following the tag" + Environment.NewLine + "with a ':' and a number.", charData);

                            DialogResult result = errorHandler.ShowDialog();

                            if (result == DialogResult.OK)
                            {
                                //MessageBox.Show("works");

                                charData = errorHandler.GetCorrectedData();

                                ProofReadTagstoCodes();

                                return;
                            }
                        }
                        break;

                    case "dismiss":
                        if (tagArgs.Length > 1)
                        {
                            bool tagSafe = CheckTagNumArgs(tagArgs[1], tagArgs[0]);

                            if (tagSafe == false)
                            {
                                return;
                            }

                            code.Add(0x1A);
                            code.Add(0x07);
                            code.Add(0x00);
                            code.Add(0x00);
                            code.Add((byte)SevenByteTypes.Dismiss);

                            byte[] tempShort = BitConverter.GetBytes(Convert.ToInt16(tagArgs[1]));

                            code.Add(tempShort[1]);
                            code.Add(tempShort[0]);
                        }

                        else
                        {
                            TagErrorHandler errorHandler = new TagErrorHandler("Missing Argument", "A tag '" + tagArgs[0] + "' requires a numerical argument." + Environment.NewLine + "Please add an argument by following the tag" + Environment.NewLine + "with a ':' and a number.", charData);

                            DialogResult result = errorHandler.ShowDialog();

                            if (result == DialogResult.OK)
                            {
                                //MessageBox.Show("works");

                                charData = errorHandler.GetCorrectedData();

                                ProofReadTagstoCodes();

                                return;
                            }
                        }
                        break;

                    case "dummy":
                        if (tagArgs.Length > 1)
                        {
                            bool tagSafe = CheckTagNumArgs(tagArgs[1], tagArgs[0]);

                            if (tagSafe == false)
                            {
                                return;
                            }

                            code.Add(0x1A);
                            code.Add(0x07);
                            code.Add(0x00);
                            code.Add(0x00);
                            code.Add((byte)SevenByteTypes.Dummy);

                            byte[] tempShort = BitConverter.GetBytes(Convert.ToInt16(tagArgs[1]));

                            code.Add(tempShort[1]);
                            code.Add(tempShort[0]);
                        }

                        else
                        {
                            TagErrorHandler errorHandler = new TagErrorHandler("Missing Argument", "A tag '" + tagArgs[0] + "' requires a numerical argument." + Environment.NewLine + "Please add an argument by following the tag" + Environment.NewLine + "with a ':' and a number.", charData);

                            DialogResult result = errorHandler.ShowDialog();

                            if (result == DialogResult.OK)
                            {
                                //MessageBox.Show("works");

                                charData = errorHandler.GetCorrectedData();

                                ProofReadTagstoCodes();

                                return;
                            }
                        }
                        break;

                    case "wait":
                        if (tagArgs.Length > 1)
                        {
                            bool tagSafe = CheckTagNumArgs(tagArgs[1], tagArgs[0]);

                            if (tagSafe == false)
                            {
                                return;
                            }

                            code.Add(0x1A);
                            code.Add(0x07);
                            code.Add(0x00);
                            code.Add(0x00);
                            code.Add((byte)SevenByteTypes.Wait);

                            byte[] tempShort = BitConverter.GetBytes(Convert.ToInt16(tagArgs[1]));

                            code.Add(tempShort[1]);
                            code.Add(tempShort[0]);
                        }

                        else
                        {
                            TagErrorHandler errorHandler = new TagErrorHandler("Missing Argument", "A tag '" + tagArgs[0] + "' requires a numerical argument." + Environment.NewLine + "Please add an argument by following the tag" + Environment.NewLine + "with a ':' and a number.", charData);

                            DialogResult result = errorHandler.ShowDialog();

                            if (result == DialogResult.OK)
                            {
                                //MessageBox.Show("works");

                                charData = errorHandler.GetCorrectedData();

                                ProofReadTagstoCodes();

                                return;
                            }
                        }
                        break;

                    default:
                        TagErrorHandler errorHandlerDefault = new TagErrorHandler("Unknown Tag", "An unknown tag '" + tagArgs[0] + "' was found." + Environment.NewLine + "Please correct this error.", charData);
                        
                        DialogResult resultDefault = errorHandlerDefault.ShowDialog();
                        
                        if (resultDefault == DialogResult.OK)
                        {
                        //MessageBox.Show("works");

                        charData = errorHandlerDefault.GetCorrectedData();

                        ProofReadTagstoCodes();

                        return;
                        }

                        return;
                    #endregion
                }

                testList.InsertRange((int)(i + tagSize), code.ToArray());

                testList.RemoveRange(i, (int)tagSize);

                i += code.Count - 1;
            }
        }

        public bool CheckTagNumArgs(string arg, string tagName)
        {
            char[] tempCharArray = arg.ToCharArray();

            bool argIsSafe = false;

            if (tempCharArray.Length == 0)
            {
                TagErrorHandler errorHandler = new TagErrorHandler("Numerical Argument Error", "The tag '" + tagName + "' requires a numerical argument." + Environment.NewLine + "Please enter a number after the ':'.", charData);

                DialogResult result = errorHandler.ShowDialog();

                if (result == DialogResult.OK)
                {
                    //MessageBox.Show("works");

                    charData = errorHandler.GetCorrectedData();

                    ProofReadTagstoCodes();

                    argIsSafe = false;

                    return argIsSafe;
                }
            }

            foreach (char chara in tempCharArray)
            {
                if (char.IsDigit(chara) != true)
                {
                    TagErrorHandler errorHandler = new TagErrorHandler("Numerical Argument Error", "The tag argument " + arg + " has letters in it." + Environment.NewLine + "Please use only numbers in tag arguments.", charData);

                    DialogResult result = errorHandler.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        //MessageBox.Show("works");

                        charData = errorHandler.GetCorrectedData();

                        ProofReadTagstoCodes();

                        argIsSafe = false;

                        return argIsSafe;
                    }
                }
            }

            argIsSafe = true;

            return argIsSafe;
        }
    }

    public class BMG_New
    {
        public List<Message> messageList;

        public ushort windWakerCheck;

        public bool IsWindWaker = true;

        public ushort numEntries;

        public uint dat1Size;

        public BMG_New(EndianBinaryReader stream)
        {
            //Loads a BMG file from a file stream

            messageList = new List<Message>();

            //There are two headers at the start of the BMG file. One is
            //the main file header, which begins with the string "MESGbmg1."
            //It contains the file size and two other variables with unknown
            //purposes. The other is the INF1 header, which begins with "INF1"
            //and contains the INF1's size, the number of messages, and the
            //size of the message entries. For TWW, the message entry size is 0x18.
            //We only need the number of message entries to parse the file, so we'll
            //pull that from INF1 and disregard everything else.

            windWakerCheck = (ushort)stream.ReadInt16At(0x2A);

            if (windWakerCheck != 0x18)
            {
                IsWindWaker = false;

                return;
            }

            numEntries = (ushort)stream.ReadInt16At(0x28); 

            stream.BaseStream.Position = 0x30; 

            for (int i = 0; i < numEntries; i++)
            {
                Message mes = new Message(stream);

                //There are many null entries in the file as it is.
                //This will filter them out. Why do they exist?
                //Nintendo. That's why.
                if (mes.charOffset == 0)
                {
                    continue;
                }

                messageList.Add(mes);
            }

            dat1Size = (uint)stream.ReadInt32At((stream.BaseStream.Position + 0xC));

            stream.BaseStream.Position += 0x11;

            uint charDataSize = 0;

            for (int j = 0; j < messageList.Count; j++)
            {
                if (j == (messageList.Count - 1))
                {
                    charDataSize = (uint)(dat1Size - messageList[j].charOffset);
                }

                else
                {
                    charDataSize = (uint)(messageList[j + 1].charOffset - messageList[j].charOffset);
                }

                messageList[j].GetCharData(stream, charDataSize);
            }

            //ExportBMGFromPath(messageList);
        }

        public byte[] ExportBMGFromPath(List<Message> exportList)
        {
            MemoryStream stream = new MemoryStream();
            
            EndianBinaryWriter endianWriter = new EndianBinaryWriter(stream, Endian.Big);

            ExportBMG(exportList, endianWriter);

            byte[] fileData = stream.ToArray();

            stream.Close();

            return fileData;
        }

        public void ExportBMG(List<Message> exportList, EndianBinaryWriter endianWriter)
        {
            WriteBMGHeader(endianWriter);

            WriteINF1Header(endianWriter, exportList);

            for (int i = 0; i < exportList.Count; i++)
            {
                exportList[i].ControlTagsToCodes();

                if (exportList[i].charData[exportList[i].charData.Count - 1] != 0)
                {
                    exportList[i].charData.Add(0);
                }

                if (exportList[i].originalCharLength != exportList[i].charData.Count)
                {
                    int charDataLengthDifference = (exportList[i].charData.Count) - ((int)exportList[i].originalCharLength);

                    for (int j = i + 1; j < exportList.Count; j++)
                    {
                        exportList[j].charOffset += charDataLengthDifference;
                    }
                }
            }

            foreach (Message mes in exportList)
            {
                mes.WriteEntryData(endianWriter);
            }

            endianWriter.Write((int)0);

            endianWriter.Write((int)0);

            WriteDAT1Header(endianWriter, exportList);

            foreach (Message mes in exportList)
            {
                mes.WriteCharData(endianWriter);
            }

            uint fileSize = (uint)endianWriter.BaseStream.Position;

            endianWriter.BaseStream.Position = 8;

            endianWriter.Write((uint)fileSize);
        }

        public void WriteBMGHeader(EndianBinaryWriter endianWriter)
        {
            endianWriter.WriteFixedString((string)"MESGbmg1", 8);

            endianWriter.Write((int)0);

            endianWriter.Write((int)2);

            endianWriter.Write((int)0x1000000);

            endianWriter.Write((int)0);

            endianWriter.Write((int)0);

            endianWriter.Write((int)0);
        }

        public void WriteINF1Header(EndianBinaryWriter endianWriter, List<Message> exportList)
        {
            endianWriter.WriteFixedString((string)"INF1", 4);

            endianWriter.Write((int)((exportList.Count * 0x18) + 0x10 + 8));

            endianWriter.Write((short)messageList.Count);

            endianWriter.Write((short)0x18);

            endianWriter.Write((int)0);
        }

        public void WriteDAT1Header(EndianBinaryWriter endianWriter, List<Message> exportList)
        {
            uint newDat1Size = 0x09;

            foreach (Message mes in exportList)
            {
                newDat1Size += (uint)mes.charData.Count;
            }

            endianWriter.WriteFixedString("DAT1", 4);

            endianWriter.Write((uint)newDat1Size);

            endianWriter.Write((byte)0);
        }

        public List<Message> GetMessageList()
        {
            return messageList;
        }
    }
}

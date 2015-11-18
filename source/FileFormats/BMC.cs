using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GameFormatReader.Common;

namespace TextEditor2
{
    /// <summary>
    /// The BMC file holds color data for the text. A control code specifies the index of a color,
    /// and the engine changes the text's color to that one.
    /// </summary>
    /// 

    public class ByteColorAlpha
    {
        public byte R, G, B, A;

        public ByteColorAlpha(byte[] data)
        {
            R = data[0];
            G = data[1];
            B = data[2];
            A = data[3];
        }

        public ByteColorAlpha()
        {
            R = G = B = A = 0;
        }

        /*public ByteColorAlpha(ByteColor color)
        {
            R = color.R;
            G = color.G;
            B = color.B;
            A = 0;
        }*/

        public byte[] GetBytes()
        {
            byte[] bytes = new byte[4];
            bytes[0] = R;
            bytes[1] = G;
            bytes[2] = B;
            bytes[3] = A;

            return bytes;
        }
    }

    class BMCParser
    {
        public List<ByteColorAlpha> colorList;

        public BMCParser(EndianBinaryReader reader)
        {
            colorList = new List<ByteColorAlpha>();

            short numColors = reader.ReadInt16At(40);

            reader.BaseStream.Position += 44;

            //int numColors = Helpers.Read16(data, 40);

            for (int i = 0; i < numColors; i++)
            {
                ByteColorAlpha tempCol = new ByteColorAlpha();

                tempCol.R = reader.ReadByte();
                tempCol.G = reader.ReadByte();
                tempCol.B = reader.ReadByte();
                tempCol.A = reader.ReadByte();

                colorList.Add(tempCol);
            }
        }

        public List<ByteColorAlpha> GetColorList()
        {
            return colorList;
        }

        public byte[] BMCExporter(List<ByteColorAlpha> ExportList)
        {
            MemoryStream stream = new MemoryStream();

            EndianBinaryWriter writer = new EndianBinaryWriter(stream, Endian.Big);

            ExportBMC(ExportList, writer);

            byte[] fileData = stream.ToArray();

            return fileData;
        }

        public void ExportBMC(List<ByteColorAlpha> colorList, EndianBinaryWriter writer)
        {
            writer.WriteFixedString("MGCLbmc1", 8);

            writer.Write((int)34);

            writer.Write((int)1);

            for (int i = 0; i < 4; i++)
            {
                writer.Write((int)0);
            }

            writer.WriteFixedString("CLT1", 4);

            writer.Write((int)1056);

            writer.Write((short)256);

            writer.Write((short)0);

            foreach (ByteColorAlpha color in colorList)
            {
                writer.Write((byte)color.R);

                writer.Write((byte)color.G);

                writer.Write((byte)color.B);

                writer.Write((byte)color.A);
            }

            for (int i = 0; i < 5; i++)
            {
                writer.Write((int)0);
            }
        }
    }
}

using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Terraria;

namespace MultiWorldLib.Modules
{
    public class RawDataBuilder
    {
        public RawDataBuilder()
        {
            memoryStream = new();
            writer = new(memoryStream);
            writer.BaseStream.Position = 3L;
        }
        public RawDataBuilder(int packetType)
        {
            memoryStream = new();
            writer = new(memoryStream);
            writer.BaseStream.Position = 3L;
            long position = writer.BaseStream.Position;
            writer.BaseStream.Position = 2L;
            writer.Write((byte)packetType);
            writer.BaseStream.Position = position;
        }
        public RawDataBuilder SetType(byte type)
        {
            long position = writer.BaseStream.Position;
            writer.BaseStream.Position = 2L;
            writer.Write(type);
            writer.BaseStream.Position = position;
            return this;
        }

        public RawDataBuilder PackSByte(sbyte num)
        {
            writer.Write(num);
            return this;
        }

        public RawDataBuilder PackByte(byte num)
        {
            writer.Write(num);
            return this;
        }

        public RawDataBuilder PackInt16(short num)
        {
            writer.Write(num);
            return this;
        }

        public RawDataBuilder PackUInt16(ushort num)
        {
            writer.Write(num);
            return this;
        }

        public RawDataBuilder PackInt32(int num)
        {
            writer.Write(num);
            return this;
        }

        public RawDataBuilder PackUInt32(uint num)
        {
            writer.Write(num);
            return this;
        }

        public RawDataBuilder PackUInt64(ulong num)
        {
            writer.Write(num);
            return this;
        }

        public RawDataBuilder PackSingle(float num)
        {
            writer.Write(num);
            return this;
        }

        public RawDataBuilder PackString(string str)
        {
            writer.Write(str);
            return this;
        }

        public RawDataBuilder PackRGB(Color? color)
        {
            writer.WriteRGB((Color)color);
            return this;
        }
        public RawDataBuilder PackVector2(Vector2 v)
        {
            writer.Write(v.X);
            writer.Write(v.Y);
            return this;
        }

        private void UpdateLength()
        {
            long position = writer.BaseStream.Position;
            writer.BaseStream.Position = 0L;
            writer.Write((short)position);
            writer.BaseStream.Position = position;
        }

        public static string ByteArrayToString(byte[] data)
        {
            StringBuilder stringBuilder = new(data.Length * 2);
            foreach (byte b in data)
            {
                stringBuilder.AppendFormat("{0:x2}", b);
            }
            return stringBuilder.ToString();
        }

        public byte[] GetByteData()
        {
            UpdateLength();
            return memoryStream.ToArray();
        }

        public MemoryStream memoryStream;

        public BinaryWriter writer;
    }
}

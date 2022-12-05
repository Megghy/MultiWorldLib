using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;
using MultiWorldLib.Entities;
using MultiWorldLib.Modules;
using Terraria.Localization;
using Terraria.ModLoader;
using XPT.Core.Audio.MP3Sharp.Decoding;

namespace MultiWorldLib.Net
{
    public sealed class MultiWorldPacket : BinaryWriter
    {
        private byte[] buf;
        private ushort len;
        internal MultiWorldPacket(string key = null, int capacity = 256)
            : base(new MemoryStream(capacity))
        {
            Write((ushort)0);
            Write(key ?? GetType().Namespace);
        }
        internal byte[] GetBytes()
        {
            Finish();
            return buf;
        }
        /// <summary>
        /// Can only use this on Subworld, useless
        /// </summary>
        public void Send()
        {
            MWNetManager.SendCustomDataToBridge(this);
        }

        internal void Finish()
        {
            if (buf == null)
            {
                if (OutStream.Position > 65535)
                {
                    throw new Exception(Language.GetTextValue("tModLoader.MPPacketTooLarge", OutStream.Position, ushort.MaxValue));
                }

                len = (ushort)OutStream.Position;
                Seek(0, SeekOrigin.Begin);
                Write(len);
                Close();
                buf = ((MemoryStream)OutStream).GetBuffer();
            }
        }

    }
}

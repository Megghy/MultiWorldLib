using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MultiWorldLib.Entities;
using MultiWorldLib.Interfaces;
using Terraria.ModLoader;

namespace MultiWorldLib.Net.Packets
{
    public class SetClientWorldClassPacket : BaseMWPacket
    {
        public const string CALLEVENTPACKET_KEY = "MultiWorldLib.SetClientWorldClass";
        public override string Key
            => CALLEVENTPACKET_KEY;
        public string WorldClassName { get; private set; }
        public override void Read(BinaryReader reader)
        {
            WorldClassName = reader.ReadString();
        }
        public override void Write(BinaryWriter writer)
        {
            writer.Write(WorldClassName);
        }
    }
}

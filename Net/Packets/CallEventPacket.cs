using System.IO;
using MultiWorldLib.Entities;
using MultiWorldLib.Interfaces;
using Terraria.ModLoader;

namespace MultiWorldLib.Net.Packets
{
    public class CallEventPacket : BaseMWPacket
    {
        public const string CALLEVENTPACKET_KEY = "MultiWorldLib.CallEvent";
        public MWEventTypes EventType { get; set; }
        public override string Key
            => CALLEVENTPACKET_KEY;

        public override void Read(BinaryReader reader)
        {
            EventType = (MWEventTypes)reader.ReadByte();
        }
        public override void Write(BinaryWriter writer)
        {
            writer.Write((byte)EventType);
        }
    }
}

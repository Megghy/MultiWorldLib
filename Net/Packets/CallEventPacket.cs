using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiWorldLib.Entities;
using MultiWorldLib.Interfaces;
using Terraria.ModLoader;

namespace MultiWorldLib.Net.Packets
{
    public class CallEventPacket : IMWPacket<CallEventPacket>
    {
        public MWPacketTypes Type
            => MWPacketTypes.CallEvent;

        public MWEventTypes EventType { get; set; }

        public void Read(BinaryReader reader)
        {
            EventType = (MWEventTypes)reader.ReadByte();
        }

        public void Write(ModPacket packet)
        {
            packet.Write((byte)EventType);
        }
    }
}

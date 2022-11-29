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
    public class RequestJoinSubServerPacket : IMWPacket<RequestJoinSubServerPacket>
    {
        public MWPacketTypes Type => MWPacketTypes.RequestJoinSubServer;


        public RequestJoinSubServerPacket Read(BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        public void Write(ModPacket packet)
        {
            throw new NotImplementedException();
        }
    }
}

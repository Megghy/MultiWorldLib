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
    public class SetClientWorldClassPacket : IMWPacket<SetClientWorldClassPacket>
    {
        public MWPacketTypes Type
            => MWPacketTypes.SetClientWorldClass;

        public Type WorldClass { get; set; }

        public void Read(BinaryReader reader)
        {
            var className = reader.ReadString();
            WorldClass = Assembly.GetExecutingAssembly().GetType(className);
        }

        public void Write(ModPacket packet)
        {
            if (WorldClass is null)
                throw new ArgumentNullException(nameof(WorldClass));
            packet.Write(WorldClass.FullName);
        }
    }
}

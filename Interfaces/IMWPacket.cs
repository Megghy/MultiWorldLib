using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiWorldLib.Entities;
using Terraria.ModLoader;

namespace MultiWorldLib.Interfaces
{
    public interface IMWPacket<T>
    {
        public MWPacketTypes Type { get; }
        public void Read(BinaryReader reader);
        public void Write(ModPacket packet);
    }
}

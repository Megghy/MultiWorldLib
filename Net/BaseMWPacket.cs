using System.IO;
using MultiWorldLib.Entities;

namespace MultiWorldLib.Net
{
    public abstract class BaseMWPacket
    {
        public BaseMWPacket() { }
        public abstract string Key { get; }
        public abstract void Read(BinaryReader reader);
        public abstract void Write(BinaryWriter writer);
    }
}

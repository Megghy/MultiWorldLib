using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MultiWorldLib.Entities;

namespace MultiWorldLib.Interfaces
{
    public interface IMWAdapter : IDisposable
    {
        public MWPlayer Player { get; init; }
        public Task StartAsync(CancellationToken cancel = default);
        public void SendToBrige(byte[] data);
        public void SendToClient(byte[] data);
        public bool OnRecieveVanillaPacket(ref byte messageType, ref BinaryReader reader);
    }
}

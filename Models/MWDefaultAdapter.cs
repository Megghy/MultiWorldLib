using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MultiWorldLib.Entities;
using MultiWorldLib.Interfaces;

namespace MultiWorldLib.Models
{
    public class MWDefaultAdapter : IMWAdapter
    {
        public MWDefaultAdapter(MWPlayer plr) { Player = plr; }
        public MWPlayer Player { get; init; }

        public void Dispose()
        {
        }

        public bool OnRecieveVanillaPacket(ref byte messageType, ref BinaryReader reader)
        {
            return false;
        }

        public void SendToBrige(byte[] data)
        {
        }

        public void SendToBrige(BinaryReader reader)
        {
        }

        public void SendToClient(byte[] data)
        {
        }
        public void SendToClient(BinaryReader reader)
        {
        }
        public Task StartAsync(CancellationToken cancel = default)
        {
            return Task.CompletedTask;
        }
    }
}

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MultiWorldLib.Entities;
using MultiWorldLib.Net;
using Terraria;

namespace MultiWorldLib.Models
{
    public class BaseMWAdapter : IDisposable
    {
        public BaseMWAdapter(MWPlayer plr) { Player = plr; }
        public MWPlayer Player { get; protected set; }

        public virtual void Dispose() { Player = null; }

        public virtual bool OnRecieveVanillaPacket(ref byte messageType, ref BinaryReader reader)
        {
            return false;
        }

        public virtual void SendToBrige(byte[] data)
        {
            //no need
        }

        public virtual void SendToBrige(BinaryReader reader)
        {
            SendToBrige(reader.ToBytes());
        }
        public virtual void SendToBrige(BaseMWPacket packet)
        {
            SendToBrige(packet.ToBytes());
        }

        public virtual void SendToClient(byte[] data)
        {
            if (ModMultiWorld.WorldSide is MWSide.HostServer or MWSide.SubServer)
                Netplay.Clients[Player.Index].Socket.AsyncSend(data, 0, data.Length, Netplay.Clients[Player.Index].ServerWriteCallBack);
        }
        public virtual void SendToClient(BinaryReader reader)
        {
            SendToClient(reader.ToBytes());
        }
        public virtual void SendToClient(BaseMWPacket packet)
        {
            SendToClient(packet.ToBytes());
        }
        public virtual Task StartAsync(CancellationToken cancel = default)
        {
            return Task.CompletedTask;
        }
    }
}

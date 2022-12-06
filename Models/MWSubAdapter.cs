using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MultiWorldLib.Entities;
using MultiWorldLib.Interfaces;
using Terraria;

namespace MultiWorldLib.Models
{
    public sealed class MWSubAdapter : IMWAdapter
    {
        public MWSubAdapter(MWPlayer plr)
        {
            Player = plr;
        }
        public MWPlayer Player { get; init; }
        public void Dispose()
        {
        }
        public Task StartAsync(CancellationToken cancel = default)
        {
            return Task.CompletedTask;
        }

        #region SendData

        public void SendToBrige(byte[] data)
            => SendToClient(data);
        public void SendToBrige(BinaryReader reader)
            => SendToClient(reader);
        public void SendToClient(byte[] data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));
            Netplay.Clients[Player.WhoAmI]?.Socket.AsyncSend(data, 0, data.Length, null);
        }
        public void SendToClient(BinaryReader reader)
        {
            SendToClient(reader.ToBytes());
        }
        #endregion

        public bool OnRecieveVanillaPacket(ref byte messageType, ref BinaryReader reader)
        {
            return false;
        }
    }
}

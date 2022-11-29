using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MultiWorldLib.Entities;
using MultiWorldLib.Interfaces;
using Terraria;
using Terraria.ID;
using Terraria.Localization;

namespace MultiWorldLib.Models
{
    public class MWSubAdapter : IMWAdapter
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
            Netplay.Clients[Player.Index]?.Socket.AsyncSend(data, 0, data.Length, null);
        }
        public void SendToClient(BinaryReader reader)
        {
            var postition = reader.BaseStream.Position;
            reader.BaseStream.Position = 0L;
            SendToClient(reader.ReadBytes((int)reader.BaseStream.Length));
            reader.BaseStream.Position = postition;
        }
        #endregion

        public bool OnRecieveVanillaPacket(ref byte messageType, ref BinaryReader reader)
        {
            switch (messageType)
            {
                case MessageID.Hello:
                    string key = reader.ReadString();
                    if (key == ModMultiWorld.CONNECTION_KEY)
                    {
                        try
                        {
                            if(JsonSerializer.Deserialize<ConnectInfo>(reader.ReadString()) is { } connect  //1 connectionInfo
                                && JsonSerializer.Deserialize<MWWorldInfo>(reader.ReadString()) is { } world) 
                            {

                            }
                        }
                        catch(Exception ex)
                        {
                            NetMessage.BootPlayer(Player.Index, NetworkText.FromLiteral($"Invalid conntection data."));
                            ModMultiWorld.Log.Error(ex);
                        }
                    }
                    break;
            }
            return false;
        }
    }
}

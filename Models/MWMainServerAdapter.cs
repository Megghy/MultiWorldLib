using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MultiWorldLib.Entities;
using MultiWorldLib.Interfaces;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using static Humanizer.In;

namespace MultiWorldLib.Models
{
    public class MWMainServerAdapter : IMWAdapter
    {
        public MWMainServerAdapter(int port, MWContainer container)
        {
            _port = port;
            WorldContainer = container;
            _tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        private readonly int _port;
        private bool _isRunning = false;
        private bool _isEntered = false;

        public MWContainer WorldContainer { get; init; }
        internal Socket _tcpClient { get; init; }
        internal NetworkStream _netStream { get; private set; }
        public MWPlayer Player { get; init; }

        public async Task StartAsync(CancellationToken cancel = default)
        {
            cancel = cancel == default ? new CancellationTokenSource(10 * 1000).Token : cancel;
            await _tcpClient.ConnectAsync(IPAddress.Parse("127.0.0.1"), _port, cancel);
            _netStream = new(_tcpClient);
        }
        public async Task TryConnectAsync()
        {
            if (!_isRunning)
                await StartAsync();
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            Task.Factory.StartNew(RecieveLoop);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            SendToBrige(new RawDataBuilder(1)
                            .PackString(ModMultiWorld.CONNECTION_KEY) //key
                            .PackString(JsonSerializer.Serialize(new ConnectInfo()
                            {
                                SubServerId = WorldContainer.Id,
                                IP = Netplay.Clients[Player.Index].Socket.GetRemoteAddress().ToString()
                            }))
                            .GetByteData()); //发起连接请求
        }
        public void Dispose()
        {
            _isRunning = false;
            _netStream?.Dispose();
            _tcpClient?.Dispose();
        }

        #region Send
        public async Task SendAsync(byte[] data)
        {
            if (_netStream is null)
                return;
            await _netStream.WriteAsync(data);
        }

        public void SendToBrige(byte[] data)
        {
            _netStream?.Write(data);
        }

        public void SendToBrige(BinaryReader reader)
        {
            reader.BaseStream.CopyTo(_netStream);
        }
        public void SendToClient(byte[] data)
        {
            Player.SendData(data);
        }
        public void SendToClient(BinaryReader reader)
        {
            Player.SendData(reader.ReadBytes((int)reader.BaseStream.Length));
        }
        #endregion

        public void RecieveLoop()
        {
            var buffer = new byte[MessageBuffer.readBufferMax];
            try
            {
                while (_isRunning && _tcpClient.Connected)
                {
                    CheckBuffer(_tcpClient.Receive(buffer), buffer);
                }
            }
            catch (SocketException ex)
            {
                if (_isEntered)
                {
                    MultiWorldAPI.BackToMainServer(Player);
                    Player?.SendInfoMsg($"Unknown error: {ex}");
                }
            }
            catch (Exception ex)
            {
                ModMultiWorld.Log.Warn($"Host recieve packet error: {ex}");
            }
        }
        void CheckBuffer(int size, byte[] buffer)
        {
            try
            {
                if (size <= 0)
                    return;
                var length = BitConverter.ToUInt16(buffer, 0);
                if (size > length)
                {
                    var position = 0;
                    while (position < size)
                    {
                        var tempLength = BitConverter.ToUInt16(buffer, position);
                        if (tempLength == 0)
                            break;
                        if (OnRecieveVanillaPacketFromSubServer(buffer, position, tempLength))
                            Array.Clear(buffer, position, tempLength);
                        position += tempLength;
                    }
                }
                else if (OnRecieveVanillaPacketFromSubServer(buffer, 0, size))
                    return;
                Netplay.Clients[Player.Index].Socket.AsyncSend(buffer, 0, size, Netplay.Clients[Player.Index].ServerWriteCallBack);
            }
            catch { }
        }
        public bool OnRecieveVanillaPacket(ref byte messageType, ref BinaryReader reader)
        {
            switch (messageType)
            {
                case MessageID.SyncEquipment:
                    if (Player.IgnoreSyncInventoryPacket)
                    {
                        return true;
                    }
                    return true;
            }
            SendToBrige(reader);
            return true;
        }
        public bool OnRecieveVanillaPacketFromSubServer(byte[] tempBuffer, int startIndex, int length)
        {
            switch (tempBuffer[startIndex + 2])
            {
                case MessageID.Kick: //kick
                    Dispose();
                    using (var reader = new BinaryReader(new MemoryStream(tempBuffer, startIndex, length)))
                    {
                        reader.BaseStream.Position = 3L;
                        var reason = NetworkText.Deserialize(reader);
                        Terraria.Chat.ChatHelper.SendChatMessageToClient(reason, Color.Orange, Player.Index);
                        ModMultiWorld.Log.Info($"{Player.Player.name} kicked from {WorldContainer}, reason: {reason}");
                        MultiWorldAPI.BackToMainServer(Player);
                        return true;
                    }
                case MessageID.PlayerInfo: //slot
                    Player.Info.Index = tempBuffer[startIndex + 3];
                    return false;
                case MessageID.WorldData:
                    Player.Info.SpawnX = BitConverter.ToInt16(tempBuffer, startIndex + 13);
                    Player.Info.SpawnY = BitConverter.ToInt16(tempBuffer, startIndex + 15) - 3;
                    if (!_isEntered)
                    {
                        SendToBrige(new RawDataBuilder(MessageID.SpawnTileData)
                            .PackInt32(WorldContainer.WorldConfig.SpawnX)
                            .PackInt32(WorldContainer.WorldConfig.SpawnY)
                            .GetByteData());
                        SendToBrige(new RawDataBuilder(MessageID.PlayerSpawn)
                            .PackByte((byte)Player.Info.Index) //player slot 
                            .PackInt16((short)WorldContainer.WorldConfig.SpawnX) //default spawn
                            .PackInt16((short)WorldContainer.WorldConfig.SpawnY)
                            .PackInt32(0) //?
                            .PackByte(1)
                            .GetByteData());
                        if (!MWHooks.OnPostSwitch(Player, WorldContainer, out _))
                        {
                            if (WorldContainer.WorldConfig.SpawnX == -1 || WorldContainer.WorldConfig.SpawnY == -1)
                                SendToClient(new RawDataBuilder(65)
                                    .PackByte(new BitsByte())
                                    .PackInt16((short)Player.Info.Index)
                                    .PackSingle((float)Player.Info.SpawnX * 16)
                                    .PackSingle((float)Player.Info.SpawnY * 16)
                                    .PackByte(1)
                                    .GetByteData());
                            else
                                SendToClient(new RawDataBuilder(65)
                                    .PackByte(new BitsByte())
                                    .PackInt16((short)Player.Info.Index)
                                    .PackSingle((float)WorldContainer.WorldConfig.SpawnX * 16)
                                    .PackSingle((float)WorldContainer.WorldConfig.SpawnY * 16)
                                    .PackByte(1)
                                    .GetByteData());
                            Player.State = PlayerState.InSubServer;
                        }
                        return true;
                    }
                    return false;
                case MessageID.RequestPassword:
                    if (Player.State == PlayerState.InSubServer)
                        return true;
                    if (Player.State == PlayerState.RequirePassword)
                    {
                        Player.SendInfoMsg($"Invalid password.");
                    }
                    else
                    {
                        Player.State = PlayerState.RequirePassword;
                        Player.SendInfoMsg($"You need a password to join this world.");
                    }
                    return true;
            }
            return false;
        }
    }
}

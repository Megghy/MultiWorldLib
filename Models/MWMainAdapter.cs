using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MultiWorldLib.Entities;
using MultiWorldLib.Interfaces;
using MultiWorldLib.Modules;
using MultiWorldLib.Net;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Net.Sockets;

namespace MultiWorldLib.Models
{
    public class MWMainAdapter : IMWAdapter
    {
        private readonly static FieldInfo _clientConnection = typeof(TcpSocket).GetField("_connection", BindingFlags.Instance | BindingFlags.NonPublic);
        public MWMainAdapter(MWPlayer plr, MWContainer container)
        {
            World = container;
            _tcpClient = new TcpClient();
            Player = plr;
            _vanillaTcpClient = (TcpClient)_clientConnection.GetValue(Netplay.Clients[plr.WhoAmI].Socket);
        }
        private bool _isRunning = false;
        private bool _isEntered = false;

        public MWPlayer Player { get; init; }
        public MWContainer World { get; init; }
        internal TcpClient _tcpClient { get; init; }
        internal TcpClient _vanillaTcpClient { get; private set; }

        public async Task StartAsync(CancellationToken cancel = default)
        {
            if (_isRunning)
                return;
            _isRunning = true;

            cancel = cancel == default ? new CancellationTokenSource(30 * 1000).Token : cancel;
            await Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        _tcpClient.Connect(IPAddress.Parse("127.0.0.1"), World.Port);
                        break;
                    }
                    catch { }
                }
            }, cancel);
        }
        public async Task TryConnectAsync(CancellationToken cancel = default)
        {
            if (!_isRunning)
                await StartAsync(cancel);
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            Task.Factory.StartNew(RecieveLoop);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            ModMultiWorld.Log.Debug($"Send Hello packet with index: {Player?.WhoAmI}");
            SendToBrigeDirect(new RawDataBuilder(1)
                            .PackString(ModMultiWorld.CONNECTION_KEY) //key
                            .PackString(JsonSerializer.Serialize(new ConnectInfo()
                            {
                                SubServerId = World.Id,
                                IP = Netplay.Clients[Player?.WhoAmI ?? 255]?.Socket.GetRemoteAddress().ToString() ?? "127.0.0.1"
                            }))
                            .PackString(JsonSerializer.Serialize(World?.WorldConfig?.Settings))
                            .GetByteData()); //发起连接请求
            while (!_isEntered && _isRunning)
            {
                cancel.ThrowIfCancellationRequested();
                Thread.Sleep(1);
            }
        }
        public void Dispose()
        {
            _isRunning = false;
            _tcpClient?.Dispose();
            _vanillaTcpClient = null;
            _fixdCheckedBuf = null;
        }

        #region Send

        public void SendToBrige(byte[] data)
        {
            if (!_isEntered)
                return;
            SendToBrigeDirect(data);
        }
        public void SendToBrigeDirect(byte[] data)
        {
            if (!_isRunning)
                return;
            var messageType = data[2];
            try
            {
                if (_isEntered
                || messageType <= 12
                || messageType == MessageID.SocialHandshake
                || messageType == MessageID.PlayerLifeMana
                || messageType == MessageID.PlayerMana
                || messageType == MessageID.PlayerBuffs
                || messageType == MessageID.SendPassword
                || messageType == MessageID.ClientUUID
                || messageType >= 249)
                {
                    _tcpClient.GetStream().Write(data);
                }
            }
            catch (SocketException ex)
            {
                MultiWorldAPI.BackToMainServer(Player);
                Player?.SendInfoMsg($"Unknown error: {ex}");
            }
        }
        public void SendToClient(Span<byte> data)
        {
            _vanillaTcpClient?.GetStream()?.Write(data);
        }
        public void SendToClient(byte[] data)
        {
            SendToClient(data.AsSpan());
        }
        #endregion
        public void RecieveLoop()
        {
            try
            {
                byte[] _defaultBuf = new byte[MessageBuffer.readBufferMax];
                while (_isRunning && _tcpClient.Connected)
                {
                    CheckBuffer(_tcpClient.GetStream().Read(_defaultBuf), _defaultBuf);
                }
            }
            catch (Exception ex) when (ex is ObjectDisposedException or SocketException)
            {
                MultiWorldAPI.BackToMainServer(Player);
                Player?.SendInfoMsg($"Unknown error: {ex}");
            }
            catch (Exception ex)
            {
                ModMultiWorld.Log.Warn($"Host recieve packet error: {ex}");
            }
        }
        private byte[] _fixdCheckedBuf = new byte[MessageBuffer.readBufferMax];
        void CheckBuffer(int size, Span<byte> buf)
        {
            if (size <= 0)
                return;
            var length = BitConverter.ToUInt16(buf[..2]);
            if (size > length)
            {
                var checkedPos = 0;
                Span<byte> _checkedBuf = new(_fixdCheckedBuf);
                var tempPos = 0;
                while (tempPos < size)
                {
                    var tempLen = BitConverter.ToUInt16(buf.Slice(tempPos, 2));
                    if (tempLen == 0)
                        break;
                    if (!OnRecieveVanillaPacketFromSubServer(buf.Slice(tempPos, tempLen)))
                    {
                        buf.Slice(tempPos, tempLen).CopyTo(_checkedBuf.Slice(checkedPos, tempLen));
                        checkedPos += tempLen;
                    }
                    tempPos += tempLen;
                }
                if (checkedPos > 0)
                    SendToClient(buf[..checkedPos]);
                _checkedBuf.Clear();
            }
            else if (!OnRecieveVanillaPacketFromSubServer(buf[..size]))
                SendToClient(buf[..size]);
        }
        public bool OnRecieveVanillaPacket(ref byte messageType, ref BinaryReader reader)
        {
            if (!_tcpClient.Connected)
                return true;
            var start = reader.BaseStream.Position -= 3;
            switch (messageType)
            {
                case MessageID.Unused15:
                    OnRecieveCustomData(reader, true);
                    break;
                case MessageID.SyncEquipment:
                    if (Player.IgnoreSyncInventoryPacket)
                    {
                        return true;
                    }
                    break;
            }
            reader.BaseStream.Position = start;
            var len = reader.ReadInt16();
            if (start >= 0 && len > 2)
                SendToBrigeDirect(reader.ToBytes((int)start, len));
            return true;
        }
        public bool OnRecieveVanillaPacketFromSubServer(Span<byte> buf)
        {
            if (ModMultiWorld.WorldSide is MWSide.MainServer)
                return OnRecieveVanillaPacketFromSubServerMultiPlay(buf);
            else
                return OnRecieveVanillaPacketFromSubServerLocalPlay(buf);
        }
        private void OnRecieveCustomData(BinaryReader reader, bool fromClient)
        {
            var type = (MWCustomTypes)reader.ReadByte();
            switch (type)
            {
                case MWCustomTypes.RequestBackToMainServer:
                    MultiWorldAPI.BackToMainServer(Player);
                    break;
            }
        }
        private bool OnRecieveVanillaPacketFromSubServerMultiPlay(Span<byte> buf)
        {
            switch (buf[2])
            {
                case MessageID.Unused15:
                    {
                        using var customReader = new BinaryReader(new MemoryStream(buf[3..].ToArray()));
                        OnRecieveCustomData(customReader, false);
                    }
                    break;
                case MessageID.Kick: //kick
                    Dispose();
                    using (var reader = new BinaryReader(new MemoryStream(buf.ToArray(), 0, buf.Length)))
                    {
                        reader.BaseStream.Position = 3L;
                        var reason = NetworkText.Deserialize(reader);
                        Terraria.Chat.ChatHelper.SendChatMessageToClient(reason, Color.Orange, Player.WhoAmI);
                        ModMultiWorld.Log.Info($"{Player.Player.name} kicked from {World}, reason: {reason}");
                        MultiWorldAPI.BackToMainServer(Player);
                        return true;
                    }
                case MessageID.PlayerInfo: //slot
                    Player.SubIndex = buf[3];
                    ModMultiWorld.Log.Debug($"{Player.Player.name} recieve packet PlayerSlot<3>");
                    break;
                case MessageID.WorldData:
                    Player.Info.SpawnX = BitConverter.ToInt16(buf.Slice(13, 2));
                    Player.Info.SpawnY = BitConverter.ToInt16(buf.Slice(15, 2)) - 3;
                    break;
                case MessageID.RequestPassword:
                    if (Player.State == PlayerState.InSubServer)
                        return true;
                    ModMultiWorld.Log.Debug($"{Player.Player.name} recieve packet RequestPassword<37>");
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
                case MessageID.FinishedConnectingToServer:
                    if (!_isEntered)
                    {
                        _isEntered = true;
                        Player.State = PlayerState.InSubServer;
                    }
                    break;
            }
            return false;
        }
        private bool OnRecieveVanillaPacketFromSubServerLocalPlay(Span<byte> buf)
        {
            return false;
        }
    }
}

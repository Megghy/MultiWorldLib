using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using MultiWorldLib.Entities;
using MultiWorldLib.Exceptions;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Net.Sockets;

namespace MultiWorldLib.Net
{
    public static class MWNetManager
    {
        private readonly static MethodInfo _syncModMethod = typeof(ModNet).GetMethod("SyncMods", BindingFlags.Static | BindingFlags.NonPublic);
        internal static bool OnRecieveVanillaPacket(ref byte messageType, ref BinaryReader reader, int playerNumber)
        {
            if (ModMultiWorld.WorldSide is MWSide.SubServer && messageType == MessageID.Hello)
            {
                string key = reader.ReadString();
                ModMultiWorld.Log.Debug("recieve connection with key: " + key);
                if (key == ModMultiWorld.CONNECTION_KEY)
                {
                    try
                    {
                        if (JsonSerializer.Deserialize<ConnectInfo>(reader.ReadString()) is { } connect  //1 connectionInfo
                            && JsonSerializer.Deserialize<MWWorldSetting>(reader.ReadString()) is { } world)
                        {
                            if (string.IsNullOrEmpty(Netplay.ServerPassword))
                            {
                                Netplay.Clients[playerNumber].State = 1;
                                _syncModMethod.Invoke(null, new object[] { playerNumber });
                            }
                            else
                            {
                                Netplay.Clients[playerNumber].State = -1;
                                NetMessage.TrySendData(37, playerNumber);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        NetMessage.BootPlayer(playerNumber, NetworkText.FromLiteral($"Invalid conntection data."));
                        ModMultiWorld.Log.Error(ex);
                    }
                }
                else
                {
                    NetMessage.BootPlayer(playerNumber, NetworkText.FromLiteral($"Invalid conntection key: {key}"));
                    ModMultiWorld.Log.Warn($"{Netplay.Clients[playerNumber]?.Socket.GetRemoteAddress()} trying to connect with invalid key: {key}");
                }
                return true;
            }
            else
            {
                if (!Main.dedServ)
                {
                    var start = reader.BaseStream.Position -= 3;
                    var len = reader.ReadInt16();
                    var data = reader.ToBytes((int)start, len);
                    ModMultiWorld.Log.Debug($"Recieve: type: {MessageID.GetName(messageType)}{messageType}, len: {data.Length}\r\n{string.Join(" ", data.Select(b => b.ToString()))}");

                }
                if (playerNumber is > 255 or < 0)
                    return false;
                if (MWPlayer.Get(Main.player[playerNumber]) is { } plr)
                {
                    return plr.WorldAdapter?.OnRecieveVanillaPacket(ref messageType, ref reader) ?? false;
                }
                return false;
            }
        }
        internal static void OnRecievePacket(BinaryReader reader, int playerNumber)
        {
            try
            {
                var side = ModMultiWorld.WorldSide;
                var type = (MWPacketTypes)reader.ReadByte();
                switch (type)
                {
                    case MWPacketTypes.CallEvent:
                        var eventType = (MWEventTypes)reader.ReadByte();
                        if (side is MWSide.Client or MWSide.SubServer) //仅由客户端和子世界处理
                            switch (eventType)
                            {
                                case MWEventTypes.PreSwtich:
                                    ModMultiWorld.CurrentWorld?.PreEnter(playerNumber);
                                    ModMultiWorld.Log.Debug($"<PreEnter> WorldInfo - " +
                                            $"Name: {Main.worldName}, " +
                                            $"Id: {Main.worldID}, " +
                                            $"UniqueId: {(Main.ActiveWorldFileData.UseGuidAsMapName ? Main.ActiveWorldFileData.UniqueId : "[OFF]")}");
                                    if(ModMultiWorld.WorldSide is MWSide.Client)
                                    {
                                        Main.Map.Clear();
                                        Main.menuMode = 14;
                                    }
                                    break;
                                case MWEventTypes.PostSwitch:
                                    ModMultiWorld.CurrentWorld?.PostEnter(playerNumber);
                                    if (ModMultiWorld.WorldSide is MWSide.Client)
                                    {
                                        Main.Map.Load();
                                        ModMultiWorld.Log.Info($"Successfully join world: {Main.worldName}");
                                        ModMultiWorld.Log.Debug($"<PostEnter> WorldInfo - " +
                                            $"Name: {Main.worldName}, " +
                                            $"Id: {Main.worldID}, " +
                                            $"UniqueId: {(Main.ActiveWorldFileData.UseGuidAsMapName ? Main.ActiveWorldFileData.UniqueId : "[OFF]")}");
                                        Main.menuMode = 0;
                                    }
                                    break;
                                case MWEventTypes.Leave:
                                    ModMultiWorld.CurrentWorld?.OnLeave(playerNumber);
                                    break;
                                case MWEventTypes.Exit:
                                    ModMultiWorld.CurrentWorld?.OnUnload();
                                    break;
                            }
                        break;
                    case MWPacketTypes.SetClientWorldClass:
                        var className = reader.ReadString();
                        if (ModMultiWorld.WorldSide is MWSide.Client)
                        {
                            //WorldGen.clearWorld();
                            Netplay.Connection.State = 3;
                            ModMultiWorld.ActiveWorld(className);
                            ModMultiWorld.Log.Debug($"WorldReset - " +
                                $"Name: {Main.worldName}, " +
                                $"Id: {Main.worldID}, " +
                                $"UniqueId: {(Main.ActiveWorldFileData.UseGuidAsMapName ? Main.ActiveWorldFileData.UniqueId : "[OFF]")}");
                        }
                        break;
                }
            }
            catch (Exception ex) { ModMultiWorld.Log.Error(ex); }
            if (ModMultiWorld.WorldSide is MWSide.MainServer && Utils.GetMWPlayer(playerNumber) is { } plr && plr.IsInSubWorld)
            {
                plr.WorldAdapter?.SendToBrige(reader.ToBytes(false));
            }
        }
        internal static void OnSendBytes(On.Terraria.Net.Sockets.TcpSocket.orig_Terraria_Net_Sockets_ISocket_AsyncSend orig, TcpSocket self, byte[] data, int offset, int size, SocketSendCallback callback, object state)
        {
            if(!(ModMultiWorld.WorldSide is MWSide.MainServer
                && Utils.GetMWPlayer(Netplay.Clients.FirstOrDefault(c => c.Socket == self)?.Id) is { } plr
                && plr.State > PlayerState.InMainServer)) //如果在其他服忽略所有
            {
                orig(self, data, offset, size, callback, state);
            }
        }

        private static ConstructorInfo _modPacketConstructor = typeof(ModPacket).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First();
        private static FieldInfo _modPacketNetId = typeof(ModPacket).GetField("netID", BindingFlags.NonPublic | BindingFlags.Instance);
        public static ModPacket GetMWPacket(MWPacketTypes type)
        {
            ModPacket packet;
            try
            {
                packet = ModMultiWorld.Instance.GetPacket();
            }
            catch
            {
                packet = (ModPacket)_modPacketConstructor.Invoke(new object[] { 250, 261 });
                packet.Write((byte)0);
                _modPacketNetId.SetValue(packet, 0);
            }
            packet.Write((byte)type);
            return packet;
        }

        internal static void SendPakcetToClient(this MWPlayer plr, ModPacket packet)
        {
            if (packet is null)
                throw new ArgumentNullException(nameof(packet));
            if (ModMultiWorld.WorldSide == MWSide.Client)
                return;
            if (plr.WorldAdapter is null)
                switch (ModMultiWorld.WorldSide)
                {
                    case MWSide.MainServer:
                    case MWSide.SubServer:
                        packet.Send();
                        break;
                    case MWSide.LocalHost://todo
                        break;
                }
            else
                plr.WorldAdapter?.SendToClient(packet.ToBytes());
        }
        internal static void SendPakcetToBrige(this MWPlayer plr, ModPacket packet)
        {
            if (packet is null)
                throw new ArgumentNullException(nameof(packet));
            plr.WorldAdapter?.SendToBrige(packet.ToBytes());
        }

        /// <summary>
        /// This will sync event to client
        /// </summary>
        /// <param name="type"></param>
        /// <param name="plr"></param>
        internal static void OnCallEvent(MWEventTypes type, MWPlayer plr)
        {
            ThrowHelper.CheckSide(MWSide.MainServer | MWSide.LocalHost);

            var packet = GetMWPacket(MWPacketTypes.CallEvent);
            packet.Write((byte)type);

            plr.SendPakcetToClient(packet);
            plr.SendPakcetToBrige(packet);
        }
    }
}

﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using MultiWorldLib.Entities;
using MultiWorldLib.Exceptions;
using MultiWorldLib.Models;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Net.Sockets;

namespace MultiWorldLib.Net
{
    public static class MWNetManager
    {
        internal readonly static MethodInfo _syncModMethod = typeof(ModNet).GetMethod("SyncMods", BindingFlags.Static | BindingFlags.NonPublic);
        internal static bool OnRecieveVanillaPacket(ref byte messageType, ref BinaryReader reader, int playerNumber)
        {
            if (ModMultiWorld.WorldSide is MWSide.SubServer && messageType == MessageID.Hello)
            {
                string key = reader.ReadString();
                ModMultiWorld.Log.Debug("Recieve connection request with key: " + key);
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
                if (playerNumber is > 255 or < 0)
                    return false;
                if (MWPlayer.Get(Main.player[playerNumber]) is { } plr)
                {
                    if (ModMultiWorld.WorldSide is MWSide.SubServer && plr.WorldAdapter is null)
                        plr.WorldAdapter = new MWSubAdapter(plr);
                    return plr.WorldAdapter?.OnRecieveVanillaPacket(ref messageType, ref reader) ?? false;
                }
                return false;
            }
        }
        internal static void OnSendBytes(On.Terraria.Net.Sockets.TcpSocket.orig_Terraria_Net_Sockets_ISocket_AsyncSend orig, TcpSocket self, byte[] data, int offset, int size, SocketSendCallback callback, object state)
        {
            if (!(Utils.GetMWPlayer(Netplay.Clients.FirstOrDefault(c => c.Socket == self)?.Id) is { } plr
                && plr.State > PlayerState.InMainServer)) //如果在其他服忽略所有
            {
                orig(self, data, offset, size, callback, state);
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
                                    if (ModMultiWorld.WorldSide is MWSide.Client)
                                    {
                                        Netplay.Connection.State = 1;
                                    }
                                    break;
                                case MWEventTypes.PostSwitch:
                                    ModMultiWorld.CurrentWorld?.PostEnter(playerNumber);
                                    //ModMultiWorld.Log.Info($"[{Main.player[playerNumber].name}] Successfully join world: {Main.worldName}");
                                    if (ModMultiWorld.WorldSide is MWSide.Client)
                                    {
                                        Main.Map.Clear();
                                        Main.Map.Load();
                                        ModMultiWorld.Log.Debug($"<PostEnter> WorldInfo - " +
                                            $"Name: {Main.worldName}, " +
                                            $"Id: {Main.worldID}, " +
                                            $"UniqueId: {(Main.ActiveWorldFileData.UseGuidAsMapName ? Main.ActiveWorldFileData.UniqueId : "[OFF]")}");
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
                            MultiWorldManager.ActiveWorld(className);
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

        private static readonly ConstructorInfo _modPacketConstructor = typeof(ModPacket).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First();
        private static readonly FieldInfo _modPacketNetId = typeof(ModPacket).GetField("netID", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static ModPacket GetMWPacket(MWPacketTypes type)
        {
            ModPacket packet;
            try
            {
                packet = ModMultiWorld.Instance.GetPacket();
            }
            catch
            {
                packet = (ModPacket)_modPacketConstructor.Invoke(new object[] { 250, 10 });
                packet.Write((byte)0);
                _modPacketNetId.SetValue(packet, 0);
            }
            packet.Write((byte)type);
            return packet;
        }
        internal static MultiWorldPacket GetMultiWorldPacket(string? key = null)
        {
            return new MultiWorldPacket(key);
        }

        internal static void SendPakcetToClientPlayer(this MWPlayer plr, ModPacket packet)
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
        internal static void SendPakcetToBrigePlayer(this MWPlayer plr, ModPacket packet)
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

            plr.SendPakcetToClientPlayer(packet);
            plr.SendPakcetToBrigePlayer(packet);
        }

        internal static void OnRecieveData(Guid worldId, int length, byte[] data)
        {
            if (data.Length > 2)
            {
                using var reader = new BinaryReader(new MemoryStream(data, 0, length));
                var ns = reader.ReadString();
                MultiWorldHooks.OnRecieveCustomPacket(worldId, reader, out var args);
                if (ModMultiWorld.CurrentWorld?.Namespace == ns)
                    ModMultiWorld.CurrentWorld.OnRecieveCustomPacket(args);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        /// <exception cref="WrongSideException">Called method from wrong side</exception>
        public static void SendCustomDataToBridge(MultiWorldPacket packet)
        {
            ThrowHelper.CheckSide(MWSide.SubServer);

            SendCustomDataToBridgeDirect(packet.GetBytes());
        }
        public static void SendCustomDataToBridgeDirect(byte[] data)
        {
            ThrowHelper.CheckSide(MWSide.SubServer);

            ModMultiWorld.Instance._pipeClient.Send(data);
        }
    }
}

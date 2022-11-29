using System;
using System.IO;
using System.Reflection;
using MultiWorldLib.Entities;
using Terraria;
using Terraria.ModLoader;

namespace MultiWorldLib.Net
{
    public static class MWPacketManager
    {
        public static bool OnRecieveVanillaPacket(ref byte messageType, ref BinaryReader reader, int playerNumber)
        {
            if (MWPlayer.Get(Main.player[playerNumber]) is { } plr)
            {
                return plr.MWAdapter?.OnRecieveVanillaPacket(ref messageType, ref reader) ?? false;
            }
            return false;
        }
        public static void OnRecievePacket(BinaryReader reader, int whoAmI)
        {
            if (MWPlayer.Get(Main.player[whoAmI]) is { } plr)
            {
                try
                {
                    var side = ModMultiWorld.WorldSide;
                    var type = (MWPacketTypes)reader.ReadByte();
                    switch (type)
                    {
                        case MWPacketTypes.CallEvent:
                            if (side is not MWSide.HostServer) //仅由客户端和子世界处理
                                return;
                            var eventType = (MWEventTypes)reader.ReadByte();
                            switch (eventType)
                            {
                                case MWEventTypes.PreSwtich:
                                    ModMultiWorld.CurrentWorld?.PreEnter(plr);
                                    break;
                                case MWEventTypes.PostSwitch:
                                    ModMultiWorld.CurrentWorld?.PostEnter(plr);
                                    break;
                                case MWEventTypes.Leave:
                                    ModMultiWorld.CurrentWorld?.OnLeave(plr);
                                    break;
                                case MWEventTypes.Exit:
                                    ModMultiWorld.CurrentWorld?.OnExit();
                                    break;
                            }
                            break;
                        case MWPacketTypes.SetClientWorldClass:
                            if(ModMultiWorld.WorldSide is MWSide.Client)
                            {
                                var className = reader.ReadString();
                                if(Assembly.GetExecutingAssembly().GetType(className) is { } classType)
                                {
                                    ModMultiWorld.ActiveWorld(classType);
                                }
                            }
                            break;
                    }
                }
                catch (Exception ex) { ModMultiWorld.Log.Error(ex); }
                if (ModMultiWorld.WorldSide is MWSide.HostServer && plr.IsInSubWorld)
                {
                    plr.MWAdapter.SendToBrige(reader);
                }
            }
        }

        public static ModPacket GetMWPacket(MWPacketTypes type)
        {
            var packet = ModMultiWorld.Instance.GetPacket();
            packet.Write((byte)type);
            return packet;
        }

        internal static void SendPakcetToClient(this MWPlayer plr, ModPacket packet)
        {
            if (packet is null)
                throw new ArgumentNullException(nameof(packet));
            if (ModMultiWorld.WorldSide == MWSide.Client)
                return;
            packet.Send(plr.Index);
        }
        internal static void SendPakcetToServer(this MWPlayer plr, ModPacket packet)
        {
            if (packet is null)
                throw new ArgumentNullException(nameof(packet));
            if (ModMultiWorld.WorldSide != MWSide.Client)
                return;
            packet.Send(plr.Index);
        }
        internal static void SendPakcetToBrige(this MWPlayer plr, ModPacket packet)
        {
            if (packet is null)
                throw new ArgumentNullException(nameof(packet));
            if (ModMultiWorld.WorldSide is MWSide.HostServer && plr.IsInSubWorld)
            {
                plr.MWAdapter.SendToBrige(packet.ToBytes());
            }
        }

        /// <summary>
        /// This will sync event to client
        /// </summary>
        /// <param name="type"></param>
        /// <param name="plr"></param>
        internal static void OnCallEvent(MWEventTypes type, MWPlayer plr)
        {
            if (ModMultiWorld.WorldSide is not MWSide.HostServer)
                return;
            var packet = GetMWPacket(MWPacketTypes.CallEvent);
            packet.Write((byte)type);

            plr.SendPakcetToClient(packet);
            plr.SendPakcetToBrige(packet);
        }
    }
}

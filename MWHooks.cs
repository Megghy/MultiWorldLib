using System;
using System.IO;
using MultiWorldLib.Entities;
using MultiWorldLib.Interfaces;
using MultiWorldLib.Models;
using MultiWorldLib.Net;
using Terraria.Net.Sockets;

namespace MultiWorldLib
{
    /// <summary>
    /// These hook will work on HOST server
    /// </summary>
    public static class MWHooks
    {
        public interface IMWEventArgs
        {
            public MWPlayer Player { get; }
            public bool Handled { get; set; }
        }
        public class PlayerBackToHostEventArgs : IMWEventArgs
        {
            public PlayerBackToHostEventArgs(MWPlayer player)
            {
                Player = player;
            }
            public MWPlayer Player { get; private set; }
            public bool Handled { get; set; } = false;
        }
        public class SwitchEventArgs : IMWEventArgs
        {
            public SwitchEventArgs(MWPlayer player, MWContainer targetServer, bool isPreSwitch)
            {
                Player = player;
                TargetServer = targetServer;
                PreSwitch = isPreSwitch;
            }
            public MWPlayer Player { get; private set; }
            public MWContainer TargetServer { get; private set; }
            public bool PreSwitch { get; }
            public bool Handled { get; set; } = false;
        }
        public class SyncEventArgs : IMWEventArgs
        {
            public SyncEventArgs(MWPlayer player)
            {
                Player = player;
            }
            public MWPlayer Player { get; private set; }
            public bool Handled { get; set; } = false;
        }
        /*public class RecieveCustomPacketEventArgs : IMWEventArgs
        {
            public RecieveCustomPacketEventArgs(MWPlayer player, BaseMWPacket packet)
            {
                Player = player;
                Packet = packet;
            }
            public BaseMWPacket Packet { get; private set; }
            public MWPlayer Player { get; private set; }
            public bool Handled { get; set; } = false;
        }*/
        public static class HookDelegates
        {

            public delegate void PlayerBackToHostEvent(PlayerBackToHostEventArgs args);

            public delegate void PreSwitchEvent(SwitchEventArgs args);

            public delegate void PostSwitchEvent(SwitchEventArgs args);
            public delegate void SyncEvent(SyncEventArgs args);
            //public delegate void SendBytesEvent(SendBytesEventArgs args);
            //public delegate void RecieveCustomPacketEvent(RecieveCustomPacketEventArgs args);
        }

        //public static event HookDelegates.RecieveCustomPacketEvent RecieveCustomPacket;
        public static event HookDelegates.PlayerBackToHostEvent PlayerBackToHost;
        public static event HookDelegates.PreSwitchEvent PreSwitch;
        public static event HookDelegates.PostSwitchEvent PostSwitch;
        public static event HookDelegates.SyncEvent Sync;
        //public static event HookDelegates.SendBytesEvent SendBytes;

        internal static bool OnPlayerBackToHost(MWPlayer player, out PlayerBackToHostEventArgs args)
        {
            args = new(player);
            try
            {
                PlayerBackToHost?.Invoke(args);
            }
            catch (Exception ex)
            {
                ModMultiWorld.Log.Error($"<PlayerBackToHost> Hook handling failed.{Environment.NewLine}{ex}");
            }
            return args.Handled;
        }
        internal static bool OnPreSwitch(MWPlayer player, MWContainer targetWorld, out SwitchEventArgs args)
        {
            args = new(player, targetWorld, true);
            try
            {
                PreSwitch?.Invoke(args);
            }
            catch (Exception ex)
            {
                ModMultiWorld.Log.Error($"<PreSwitch> Hook handling failed.{Environment.NewLine}{ex}");
            }
            return args.Handled;
        }
        internal static bool OnPostSwitch(MWPlayer player, MWContainer targetWorld, out SwitchEventArgs args)
        {
            args = new(player, targetWorld, false);
            try
            {
                PostSwitch?.Invoke(args);
            }
            catch (Exception ex)
            {
                ModMultiWorld.Log.Error($"<PostSwitch> Hook handling failed.{Environment.NewLine}{ex}");
            }
            return args.Handled;
        }
        internal static bool OnSync(MWPlayer player, out SyncEventArgs args)
        {
            args = new(player);
            try
            {
                Sync?.Invoke(args);
            }
            catch (Exception ex)
            {
                ModMultiWorld.Log.Error($"<Sync> Hook handling failed.{Environment.NewLine}{ex}");
            }
            return args.Handled;
        }
        /*internal static bool OnRecieveCustomPacket(MWPlayer player, BaseMWPacket reader, out RecieveCustomPacketEventArgs args)
        {
            args = new(player, reader);
            try
            {
                RecieveCustomPacket?.Invoke(args);
            }
            catch (Exception ex)
            {
                ModMultiWorld.Log.Error($"<Sync> Hook handling failed.{Environment.NewLine}{ex}");
            }
            return args.Handled;
        }*/
    }
}

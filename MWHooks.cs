using System;
using System.IO;
using MultiWorldLib.Entities;
using MultiWorldLib.Models;

namespace MultiWorldLib
{
    public static class MWHooks
    {
        public interface IMWEventArgs
        {
            public MWPlayer Player { get; }
            public bool Handled { get; set; }
        }
        public class PlayerJoinEventArgs : IMWEventArgs
        {
            public PlayerJoinEventArgs(MWPlayer player, string ip, int port, string version)
            {
                Player = player;
                IP = ip;
                Port = port;
                Version = version;
            }
            public MWPlayer Player { get; private set; }
            public string IP { get; private set; }
            public int Port { get; private set; }
            public string Version { get; set; }
            public bool Handled { get; set; } = false;
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
        public class ChatEventArgs : IMWEventArgs
        {
            public ChatEventArgs(MWPlayer player, string message)
            {
                Player = player;
                Message = message;
            }
            public MWPlayer Player { get; private set; }
            public string Message { get; set; }
            public bool Handled { get; set; } = false;
        }
        public static class HookDelegates
        {
            public delegate void PlayerJoinEvent(PlayerJoinEventArgs args);

            public delegate void PlayerBackToHostEvent(PlayerBackToHostEventArgs args);

            public delegate void PreSwitchEvent(SwitchEventArgs args);

            public delegate void PostSwitchEvent(SwitchEventArgs args);

            public delegate void ChatEvent(ChatEventArgs args);
        }

        public static event HookDelegates.PlayerJoinEvent PlayerJoin;
        public static event HookDelegates.PlayerBackToHostEvent PlayerBackToHost;
        public static event HookDelegates.PreSwitchEvent PreSwitch;
        public static event HookDelegates.PostSwitchEvent PostSwitch;
        public static event HookDelegates.ChatEvent Chat;
        internal static bool OnPlayerJoin(MWPlayer player, string ip, int port, string version, out PlayerJoinEventArgs args)
        {
            args = new(player, ip, port, version);
            try
            {
                PlayerJoin?.Invoke(args);
            }
            catch (Exception ex)
            {
                ModMultiWorld.Log.Error($"<PlayerJoin> Hook handling failed.{Environment.NewLine}{ex}");
            }
            return args.Handled;
        }
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
        internal static bool OnPreSwitch(MWPlayer player, MWContainer targetServer, out SwitchEventArgs args)
        {
            args = new(player, targetServer, true);
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
        internal static bool OnPostSwitch(MWPlayer player, MWContainer targetServer, out SwitchEventArgs args)
        {
            args = new(player, targetServer, false);
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
        internal static bool OnChat(MWPlayer player, string text, out ChatEventArgs args)
        {
            args = new(player, text);
            try
            {
                Chat?.Invoke(args);
            }
            catch (Exception ex)
            {
                ModMultiWorld.Log.Error($"<Chat> Hook handling failed.{Environment.NewLine}{ex}");
            }
            return args.Handled;
        }
    }
}

﻿using System;
using MultiWorldLib.Interfaces;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Color = Microsoft.Xna.Framework.Color;

namespace MultiWorldLib.Entities
{
    public class MWPlayer : ModPlayer
    {
        public static MWPlayer GetMWPlayer(Player tPlayer)
            => tPlayer.GetModPlayer<MWPlayer>();
        public override string ToString()
            => $"{Player.name} :{State}<{MWAdapter.Player}>";
        public IMWAdapter MWAdapter { get; internal set; }
        public PlayerState State { get; internal set; } = PlayerState.NewConnection;
        public IMWPlayerInfo Info
        {
            get
            {
                if (IsInSubWorld)
                {
                    _subPlayerInfo ??= new();
                    return _subPlayerInfo;
                }
                else
                    return _hostPlayerInfo;
            }
        }
        public bool IsSwitchingBack { get; internal set; } = false;
        public bool IgnoreSyncInventoryPacket { get; internal set; } = false;

        internal MWSubPlayerInfo _subPlayerInfo;
        internal MWHostPlayerInfo _hostPlayerInfo;

        public bool IsInSubWorld
            => MWAdapter is not null || ModMultiWorld.WorldSide == MWSide.SubServer;
        public int SubIndex
            => IsInSubWorld
                ? _subPlayerInfo?.Index ?? -1
                : -1;

        #region 常用功能
        public void SendMsg(object text, Color color = default)
        {
            color = color == default ? new Color(212, 239, 245) : color;
            Terraria.Chat.ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(text.ToString()), color, Info.Index);
        }
        public void SendSuccessMsg(object text)
        {
            SendMsg(text, new Color(120, 194, 96));
        }
        public void SendInfoMsg(object text)
        {
            SendMsg(text, new Color(216, 212, 82));
        }
        public void SendErrorMsg(object text)
        {
            SendMsg(text, new Color(195, 83, 83));
        }
        public void SendData(byte[] data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));
            if (ModMultiWorld.WorldSide is not MWSide.Client)
                Netplay.Clients[Info.Index].Socket.AsyncSend(data, 0, data.Length, Netplay.Clients[Info.Index].ServerWriteCallBack);
        }
        #endregion

        public override void PreUpdate()
        {
            if (ModMultiWorld.WorldSide == MWSide.HostServer && State == PlayerState.InSubServer)
            {
            }
            base.PreUpdate();
        }
    }
}
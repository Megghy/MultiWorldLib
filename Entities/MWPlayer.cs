using MultiWorldLib.Interfaces;
using MultiWorldLib.Models;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Color = Microsoft.Xna.Framework.Color;

namespace MultiWorldLib.Entities
{
    public class MWPlayer : ModPlayer
    {
        public static MWPlayer? Get(Player tPlayer)
            => tPlayer.GetMWPlayer();
        public override string ToString()
            => $"{Player?.name} :{State}";
        private IMWAdapter _worldAdapter;
        public IMWAdapter? WorldAdapter 
        {
            get => _tempAdapter ?? _worldAdapter; 
            internal set => _worldAdapter = value;
        }
        internal IMWAdapter _tempAdapter;
        public PlayerState State { get; internal set; } = PlayerState.NewConnection;
        public IMWPlayerInfo Info
        {
            get
            {
                if (State > PlayerState.InMainServer)
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
        public ushort WhoAmI
            => (ushort)(IsInSubWorld && ModMultiWorld.WorldSide == MWSide.MainServer
                ? Info.WhoAmI
                : Player.whoAmI);

        internal MWSubPlayerInfo _subPlayerInfo;
        internal MWHostPlayerInfo _hostPlayerInfo;

        public bool IsInSubWorld
            => _worldAdapter is not null || ModMultiWorld.WorldSide == MWSide.SubServer || State is PlayerState.InSubServer;
        public int SubIndex { get; internal set; }

        #region 常用功能
        public void SendMsg(object text, Color color = default)
        {
            color = color == default ? new Color(212, 239, 245) : color;
            Terraria.Chat.ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(text.ToString()), color, Info.WhoAmI);
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
        #endregion
    }
}

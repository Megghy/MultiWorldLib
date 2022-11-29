using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiWorldLib.Entities;
using Terraria;
using Terraria.Localization;
using Terraria.ID;
using Terraria.Net;

namespace MultiWorldLib.Modules
{
    public static class PlayerHelper
    {
        public static readonly int PiggySlots = 40;

        public static readonly int SafeSlots = PiggySlots;

        public static readonly int ForgeSlots = SafeSlots;

        public static readonly int VoidSlots = ForgeSlots;

        public static readonly int InventorySlots = 59;

        public static readonly int ArmorSlots = 20;

        public static readonly int MiscEquipSlots = 5;

        public static readonly int DyeSlots = 10;

        public static readonly int MiscDyeSlots = MiscEquipSlots;

        public static readonly int TrashSlots = 1;
        public static void SyncCharacterInfo(this Player player)
        {
            float num9 = 0f;
            for (int j = 0; j < InventorySlots; j++)
            {
                NetMessage.SendData(MessageID.PlayerMana, -1, -1, NetworkText.FromLiteral(Main.player[player.whoAmI].inventory[j].Name), player.whoAmI, num9, (int)Main.player[player.whoAmI].inventory[j].prefix);
                num9 += 1f;
            }

            for (int k = 0; k < ArmorSlots; k++)
            {
                NetMessage.SendData(MessageID.PlayerMana, -1, -1, NetworkText.FromLiteral(Main.player[player.whoAmI].armor[k].Name), player.whoAmI, num9, (int)Main.player[player.whoAmI].armor[k].prefix);
                num9 += 1f;
            }

            for (int l = 0; l < DyeSlots; l++)
            {
                NetMessage.SendData(MessageID.PlayerMana, -1, -1, NetworkText.FromLiteral(Main.player[player.whoAmI].dye[l].Name), player.whoAmI, num9, (int)Main.player[player.whoAmI].dye[l].prefix);
                num9 += 1f;
            }

            for (int m = 0; m < MiscEquipSlots; m++)
            {
                NetMessage.SendData(MessageID.PlayerMana, -1, -1, NetworkText.FromLiteral(Main.player[player.whoAmI].miscEquips[m].Name), player.whoAmI, num9, (int)Main.player[player.whoAmI].miscEquips[m].prefix);
                num9 += 1f;
            }

            for (int n = 0; n < MiscDyeSlots; n++)
            {
                NetMessage.SendData(MessageID.PlayerMana, -1, -1, NetworkText.FromLiteral(Main.player[player.whoAmI].miscDyes[n].Name), player.whoAmI, num9, (int)Main.player[player.whoAmI].miscDyes[n].prefix);
                num9 += 1f;
            }

            for (int num10 = 0; num10 < PiggySlots; num10++)
            {
                NetMessage.SendData(MessageID.PlayerMana, -1, -1, NetworkText.FromLiteral(Main.player[player.whoAmI].bank.item[num10].Name), player.whoAmI, num9, (int)Main.player[player.whoAmI].bank.item[num10].prefix);
                num9 += 1f;
            }

            for (int num11 = 0; num11 < SafeSlots; num11++)
            {
                NetMessage.SendData(MessageID.PlayerMana, -1, -1, NetworkText.FromLiteral(Main.player[player.whoAmI].bank2.item[num11].Name), player.whoAmI, num9, (int)Main.player[player.whoAmI].bank2.item[num11].prefix);
                num9 += 1f;
            }

            NetMessage.SendData(MessageID.PlayerMana, -1, -1, NetworkText.FromLiteral(Main.player[player.whoAmI].trashItem.Name), player.whoAmI, num9++, (int)Main.player[player.whoAmI].trashItem.prefix);
            for (int num12 = 0; num12 < ForgeSlots; num12++)
            {
                NetMessage.SendData(MessageID.PlayerMana, -1, -1, NetworkText.FromLiteral(Main.player[player.whoAmI].bank3.item[num12].Name), player.whoAmI, num9, (int)Main.player[player.whoAmI].bank3.item[num12].prefix);
                num9 += 1f;
            }

            for (int num13 = 0; num13 < VoidSlots; num13++)
            {
                NetMessage.SendData(MessageID.PlayerMana, -1, -1, NetworkText.FromLiteral(Main.player[player.whoAmI].bank4.item[num13].Name), player.whoAmI, num9, (int)Main.player[player.whoAmI].bank4.item[num13].prefix);
                num9 += 1f;
            }

            NetMessage.SendData(MessageID.SyncPlayer, -1, -1, NetworkText.FromLiteral(player.name), player.whoAmI);
            NetMessage.SendData(MessageID.PlayerMana, -1, -1, NetworkText.Empty, player.whoAmI);
            NetMessage.SendData(MessageID.PlayerMana, -1, -1, NetworkText.Empty, player.whoAmI);
            num9 = 0f;
            for (int num14 = 0; num14 < InventorySlots; num14++)
            {
                NetMessage.SendData(MessageID.PlayerMana, player.whoAmI, -1, NetworkText.FromLiteral(Main.player[player.whoAmI].inventory[num14].Name), player.whoAmI, num9, (int)Main.player[player.whoAmI].inventory[num14].prefix);
                num9 += 1f;
            }

            for (int num15 = 0; num15 < ArmorSlots; num15++)
            {
                NetMessage.SendData(MessageID.PlayerMana, player.whoAmI, -1, NetworkText.FromLiteral(Main.player[player.whoAmI].armor[num15].Name), player.whoAmI, num9, (int)Main.player[player.whoAmI].armor[num15].prefix);
                num9 += 1f;
            }

            for (int num16 = 0; num16 < DyeSlots; num16++)
            {
                NetMessage.SendData(MessageID.PlayerMana, player.whoAmI, -1, NetworkText.FromLiteral(Main.player[player.whoAmI].dye[num16].Name), player.whoAmI, num9, (int)Main.player[player.whoAmI].dye[num16].prefix);
                num9 += 1f;
            }

            for (int num17 = 0; num17 < MiscEquipSlots; num17++)
            {
                NetMessage.SendData(MessageID.PlayerMana, player.whoAmI, -1, NetworkText.FromLiteral(Main.player[player.whoAmI].miscEquips[num17].Name), player.whoAmI, num9, (int)Main.player[player.whoAmI].miscEquips[num17].prefix);
                num9 += 1f;
            }

            for (int num18 = 0; num18 < MiscDyeSlots; num18++)
            {
                NetMessage.SendData(MessageID.PlayerMana, player.whoAmI, -1, NetworkText.FromLiteral(Main.player[player.whoAmI].miscDyes[num18].Name), player.whoAmI, num9, (int)Main.player[player.whoAmI].miscDyes[num18].prefix);
                num9 += 1f;
            }

            for (int num19 = 0; num19 < PiggySlots; num19++)
            {
                NetMessage.SendData(MessageID.PlayerMana, player.whoAmI, -1, NetworkText.FromLiteral(Main.player[player.whoAmI].bank.item[num19].Name), player.whoAmI, num9, (int)Main.player[player.whoAmI].bank.item[num19].prefix);
                num9 += 1f;
            }

            for (int num20 = 0; num20 < SafeSlots; num20++)
            {
                NetMessage.SendData(MessageID.PlayerMana, player.whoAmI, -1, NetworkText.FromLiteral(Main.player[player.whoAmI].bank2.item[num20].Name), player.whoAmI, num9, (int)Main.player[player.whoAmI].bank2.item[num20].prefix);
                num9 += 1f;
            }

            NetMessage.SendData(MessageID.PlayerMana, player.whoAmI, -1, NetworkText.FromLiteral(Main.player[player.whoAmI].trashItem.Name), player.whoAmI, num9++, (int)Main.player[player.whoAmI].trashItem.prefix);
            for (int num21 = 0; num21 < ForgeSlots; num21++)
            {
                NetMessage.SendData(MessageID.PlayerMana, player.whoAmI, -1, NetworkText.FromLiteral(Main.player[player.whoAmI].bank3.item[num21].Name), player.whoAmI, num9, (int)Main.player[player.whoAmI].bank3.item[num21].prefix);
                num9 += 1f;
            }

            for (int num22 = 0; num22 < VoidSlots; num22++)
            {
                NetMessage.SendData(MessageID.PlayerMana, player.whoAmI, -1, NetworkText.FromLiteral(Main.player[player.whoAmI].bank4.item[num22].Name), player.whoAmI, num9, (int)Main.player[player.whoAmI].bank4.item[num22].prefix);
                num9 += 1f;
            }

            NetMessage.SendData(MessageID.PlayerMana, player.whoAmI, -1, NetworkText.FromLiteral(player.name), player.whoAmI);
            NetMessage.SendData(MessageID.PlayerMana, player.whoAmI, -1, NetworkText.Empty, player.whoAmI);
            NetMessage.SendData(MessageID.PlayerMana, player.whoAmI, -1, NetworkText.Empty, player.whoAmI);
            for (int num23 = 0; num23 < 22; num23++)
            {
                player.buffType[num23] = 0;
            }

            NetMessage.SendData(MessageID.PlayerMana, -1, -1, NetworkText.Empty, player.whoAmI);
            NetMessage.SendData(MessageID.PlayerMana, player.whoAmI, -1, NetworkText.Empty, player.whoAmI);
            NetMessage.SendData(MessageID.PlayerMana, player.whoAmI, -1, NetworkText.Empty, player.whoAmI);
            NetMessage.SendData(MessageID.PlayerMana, -1, -1, NetworkText.Empty, player.whoAmI);
            NetMessage.SendData(MessageID.PlayerMana, player.whoAmI, -1, NetworkText.Empty, MessageID.PlayerMana);
            if (!Main.GameModeInfo.IsJourneyMode)
            {
                return;
            }

            /*Terraria.GameContent.Creative.CreativePowersHelper.
            Dictionary<int, int> sacrificedItems = player.sacri;
            for (int num24 = 0; num24 < 5088; num24++)
            {
                int sacrificeCount = 0;
                if (sacrificedItems.ContainsKey(num24))
                {
                    sacrificeCount = sacrificedItems[num24];
                }

                NetPacket packet = Terraria.GameContent.NetModules.NetCreativeUnlocksModule.SerializeItemSacrifice(num24, sacrificeCount);
                NetManager.Instance.SendToClient(packet, player.whoAmI);
            }*/
        }
    }
}

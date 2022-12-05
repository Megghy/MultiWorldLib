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
    }
}

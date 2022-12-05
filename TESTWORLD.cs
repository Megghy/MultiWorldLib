using MultiWorldLib.Entities;
using MultiWorldLib.ModTypes;
using Terraria;
using Terraria.DataStructures;

namespace MultiWorldLib
{
    public class TESTBASEWORLD : BaseMultiWorld
    {
        public override ActiveSide ActiveSide { get; }
        public override string Name => "TESTWORLD";

        public override void OnLoad()
        {
            if (!Main.dedServ) //local
            {
                Main.NewText($"{GetType().FullName} now active.");
            }
        }

        public override void OnUnload()
        {
            if (!Main.dedServ) //local
            {
                Main.NewText($"{GetType().FullName} now inactive.");
            }
        }
    }
    public class TESTWEAKPLAYER : MWModPlayer
    {
        public override string[] AttachTo
            => new[] { "MultiWorldLib.TESTBASEWORLD" };
        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter)
        {
            damage = 0;
            return true;
        }
    }

    internal class TESTWORLD : MWModSystem<TESTBASEWORLD>
    {
        public int tick = 0;
        public override void PostUpdateEverything()
        {
            if (!Main.dedServ && tick % 100 == 0) //local
            {
                Main.NewText(tick);
                tick++;
            }
        }
    }
}

using MultiWorldLib.Entities;
using MultiWorldLib.ModTypes;
using Terraria;

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
    public class TESTWEAKPLAYER : MWModModBiome
    {
        public override string[] AttachTo
            => new[] { "MultiWorldLib.TESTBASEWORLD" };
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
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

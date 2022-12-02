using System;
using System.Threading.Tasks;
using MultiWorldLib.Entities;
using MultiWorldLib.ModTypes;
using Terraria;
using Terraria.ModLoader;

namespace MultiWorldLib
{
    public class TESTBASEWORLD : BaseMultiWorld
    {
        public override ActiveSide ActiveSide { get; }

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

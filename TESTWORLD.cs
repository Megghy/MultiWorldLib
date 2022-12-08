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
    internal class TESTWORLD : MultiWorldModSystem<TESTBASEWORLD>
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
    public class TESTWORLD2 : MultiWorldModSystem
    {
        public int tick = 0;

        public override string[] AttachTo
            => new[] { "ExampleMod.Abc.DefWorld", "*" }; 

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

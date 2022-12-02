using MultiWorldLib.Entities;
using Terraria.ModLoader;

namespace MultiWorldLib.ModTypes
{
    public abstract class MWModSystem<T> : ModSystem, IMWModType<T> where T : BaseMultiWorld { }
    public abstract class MWModAccessorySlot<T> : ModAccessorySlot, IMWModType<T> where T : BaseMultiWorld { }
}

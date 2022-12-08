using MultiWorldLib.Entities;
using Terraria.ModLoader;


namespace MultiWorldLib.ModTypes
{
    public abstract class MultiWorldModSystem<TAttachTo> : ModSystem, IMWModType<TAttachTo, ModSystem> where TAttachTo : BaseMultiWorld
    {
        public virtual string[] AlsoAttachTo { get; }
        public sealed override bool IsLoadingEnabled(Mod mod)
        {
            if (mod == MultiWorldAPI.CurrentWorld?.ParentMod)
                return true;
            return false;
        }
    }
    public abstract class MultiWorldModSystem : ModSystem, IMWWeakModType<ModSystem>
    {
        public abstract string[] AttachTo { get; }
        public sealed override bool IsLoadingEnabled(Mod mod)
        {
            if (mod == MultiWorldAPI.CurrentWorld?.ParentMod)
                return true;
            return false;
        }
    }

    /*public abstract class MultiWorldModAccessorySlot<TAttachTo> : ModAccessorySlot, IMWModType<TAttachTo, ModAccessorySlot> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModBlockType<TAttachTo> : ModBlockType, IMWModType<TAttachTo, ModBlockType> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //[Obsolete("Objects that need to be common between worlds cannot be modified.", true)]
    public abstract class MultiWorldModBossBar<TAttachTo> : ModBossBar, IMWModType<TAttachTo, ModBossBar> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModBossBarStyle<TAttachTo> : ModBossBarStyle, IMWModType<TAttachTo, ModBossBarStyle> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //[Obsolete("Objects that need to be common between worlds cannot be modified.", true)]
    public abstract class MultiWorldModBuff<TAttachTo> : ModBuff, IMWModType<TAttachTo, ModBuff> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModModCactus<TAttachTo> : ModCactus, IMWModType<TAttachTo, ModCactus> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //[Obsolete("Objects that need to be common between worlds cannot be modified.", true)]
    public abstract class MultiWorldModCommand<TAttachTo> : ModCommand, IMWModType<TAttachTo, ModCommand> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //[Obsolete("Objects that need to be common between worlds cannot be modified.", true)]
    public abstract class MultiWorldModDust<TAttachTo> : ModDust, IMWModType<TAttachTo, ModDust> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //[Obsolete("Objects that need to be common between worlds cannot be modified.", true)]
    public abstract class MultiWorldModItem<TAttachTo> : ModItem, IMWModType<TAttachTo, ModItem> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModMapLayer<TAttachTo> : ModMapLayer, IMWModType<TAttachTo, ModMapLayer> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModMenu<TAttachTo> : ModMenu, IMWModType<TAttachTo, ModMenu> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModMount<TAttachTo> : ModMount, IMWModType<TAttachTo, ModMount> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModNPC<TAttachTo> : ModNPC, IMWModType<TAttachTo, ModNPC> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModPalmTree<TAttachTo> : ModPalmTree, IMWModType<TAttachTo, ModPalmTree> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModPrefix<TAttachTo> : ModPrefix, IMWModType<TAttachTo, ModPrefix> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModProjectile<TAttachTo> : ModProjectile, IMWModType<TAttachTo, ModProjectile> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModPylon<TAttachTo> : ModPylon, IMWModType<TAttachTo, ModPylon> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModTexturedType<TAttachTo> : ModTexturedType, IMWModType<TAttachTo, ModTexturedType> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModTile<TAttachTo> : ModTile, IMWModType<TAttachTo, ModTile> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModTileEntity<TAttachTo> : ModTileEntity, IMWModType<TAttachTo, ModTileEntity> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModWall<TAttachTo> : ModWall, IMWModType<TAttachTo, ModWall> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModTree<TAttachTo> : ModTree, IMWModType<TAttachTo, ModTree> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModBackgroundStyle<TAttachTo> : ModBackgroundStyle, IMWModType<TAttachTo, ModBackgroundStyle> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModModBiome<TAttachTo> : ModBiome, IMWModType<TAttachTo, ModBiome> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModGore<TAttachTo> : ModGore, IMWModType<TAttachTo, ModGore> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModPlayer<TAttachTo> : ModPlayer, IMWModType<TAttachTo, ModPlayer> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModRarity<TAttachTo> : ModRarity, IMWModType<TAttachTo, ModRarity> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModSceneEffect<TAttachTo> : ModSceneEffect, IMWModType<TAttachTo, ModSceneEffect> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModSurfaceBackgroundStyle<TAttachTo> : ModSurfaceBackgroundStyle, IMWModType<TAttachTo, ModSurfaceBackgroundStyle> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModUndergroundBackgroundStyle<TAttachTo> : ModUndergroundBackgroundStyle, IMWModType<TAttachTo, ModUndergroundBackgroundStyle> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModWaterfallStyle<TAttachTo> : ModWaterfallStyle, IMWModType<TAttachTo, ModWaterfallStyle> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MultiWorldModWaterStyle<TAttachTo> : ModWaterStyle, IMWModType<TAttachTo, ModWaterStyle> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }*/

    //weakAttach ↓↓↓↓↓
    /*public abstract class MultiWorldModGore : ModGore, IMWWeakModType<ModGore> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModAccessorySlot : ModAccessorySlot, IMWWeakModType<ModAccessorySlot> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModBackgroundStyle : ModBackgroundStyle, IMWWeakModType<ModBackgroundStyle> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModModBiome : ModBiome, IMWWeakModType<ModBiome> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModBlockType : ModBlockType, IMWWeakModType<ModBlockType> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModBossBar : ModBossBar, IMWWeakModType<ModBossBar> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModBossBarStyle : ModBossBarStyle, IMWWeakModType<ModBossBarStyle> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModBuff : ModBuff, IMWWeakModType<ModBuff> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModModCactus : ModCactus, IMWWeakModType<ModCactus> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModCommand : ModCommand, IMWWeakModType<ModCommand> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModDust : ModDust, IMWWeakModType<ModDust> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModItem : ModItem, IMWWeakModType<ModItem> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModMapLayer : ModMapLayer, IMWWeakModType<ModMapLayer> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModMenu : ModMenu, IMWWeakModType<ModMenu> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModMount : ModMount, IMWWeakModType<ModMount> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModNPC : ModNPC, IMWWeakModType<ModNPC> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModPalmTree : ModPalmTree, IMWWeakModType<ModPalmTree> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModPlayer : ModPlayer, IMWWeakModType<ModPlayer> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModPrefix : ModPrefix, IMWWeakModType<ModPrefix> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModProjectile : ModProjectile, IMWWeakModType<ModProjectile> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModPylon : ModPylon, IMWWeakModType<ModPylon> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModRarity : ModRarity, IMWWeakModType<ModRarity> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModSceneEffect : ModSceneEffect, IMWWeakModType<ModSceneEffect> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModSurfaceBackgroundStyle : ModSurfaceBackgroundStyle, IMWWeakModType<ModSurfaceBackgroundStyle> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModTexturedType : ModTexturedType, IMWWeakModType<ModTexturedType> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModTile : ModTile, IMWWeakModType<ModTile> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModTileEntity : ModTileEntity, IMWWeakModType<ModTileEntity> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModTree : ModTree, IMWWeakModType<ModTree> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModUndergroundBackgroundStyle : ModUndergroundBackgroundStyle, IMWWeakModType<ModUndergroundBackgroundStyle> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModWall : ModWall, IMWWeakModType<ModWall> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModWaterfallStyle : ModWaterfallStyle, IMWWeakModType<ModWaterfallStyle> { public abstract string[] AttachTo { get; } }
    public abstract class MultiWorldModWaterStyle : ModWaterStyle, IMWWeakModType<ModWaterStyle> { public abstract string[] AttachTo { get; } }*/
}

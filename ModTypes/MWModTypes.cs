using MultiWorldLib.Entities;
using Terraria.ModLoader;

namespace MultiWorldLib.ModTypes
{
    //public abstract class MWModAccessorySlot<TAttachTo> : ModAccessorySlot, IMWModType<TAttachTo, ModAccessorySlot> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModBlockType<TAttachTo> : ModBlockType, IMWModType<TAttachTo, ModBlockType> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //[Obsolete("Objects that need to be common between worlds cannot be modified.", true)]
    //public abstract class MWModBossBar<TAttachTo> : ModBossBar, IMWModType<TAttachTo, ModBossBar> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModBossBarStyle<TAttachTo> : ModBossBarStyle, IMWModType<TAttachTo, ModBossBarStyle> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //[Obsolete("Objects that need to be common between worlds cannot be modified.", true)]
    //public abstract class MWModBuff<TAttachTo> : ModBuff, IMWModType<TAttachTo, ModBuff> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModModCactus<TAttachTo> : ModCactus, IMWModType<TAttachTo, ModCactus> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //[Obsolete("Objects that need to be common between worlds cannot be modified.", true)]
    //public abstract class MWModCommand<TAttachTo> : ModCommand, IMWModType<TAttachTo, ModCommand> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //[Obsolete("Objects that need to be common between worlds cannot be modified.", true)]
    //public abstract class MWModDust<TAttachTo> : ModDust, IMWModType<TAttachTo, ModDust> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //[Obsolete("Objects that need to be common between worlds cannot be modified.", true)]
    //public abstract class MWModItem<TAttachTo> : ModItem, IMWModType<TAttachTo, ModItem> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModMapLayer<TAttachTo> : ModMapLayer, IMWModType<TAttachTo, ModMapLayer> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModMenu<TAttachTo> : ModMenu, IMWModType<TAttachTo, ModMenu> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModMount<TAttachTo> : ModMount, IMWModType<TAttachTo, ModMount> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModNPC<TAttachTo> : ModNPC, IMWModType<TAttachTo, ModNPC> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModPalmTree<TAttachTo> : ModPalmTree, IMWModType<TAttachTo, ModPalmTree> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModPrefix<TAttachTo> : ModPrefix, IMWModType<TAttachTo, ModPrefix> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModProjectile<TAttachTo> : ModProjectile, IMWModType<TAttachTo, ModProjectile> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModPylon<TAttachTo> : ModPylon, IMWModType<TAttachTo, ModPylon> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModTexturedType<TAttachTo> : ModTexturedType, IMWModType<TAttachTo, ModTexturedType> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModTile<TAttachTo> : ModTile, IMWModType<TAttachTo, ModTile> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModTileEntity<TAttachTo> : ModTileEntity, IMWModType<TAttachTo, ModTileEntity> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModWall<TAttachTo> : ModWall, IMWModType<TAttachTo, ModWall> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModTree<TAttachTo> : ModTree, IMWModType<TAttachTo, ModTree> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MWModSystem<TAttachTo> : ModSystem, IMWModType<TAttachTo, ModSystem> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MWModBackgroundStyle<TAttachTo> : ModBackgroundStyle, IMWModType<TAttachTo, ModBackgroundStyle> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MWModModBiome<TAttachTo> : ModBiome, IMWModType<TAttachTo, ModBiome> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModGore<TAttachTo> : ModGore, IMWModType<TAttachTo, ModGore> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModPlayer<TAttachTo> : ModPlayer, IMWModType<TAttachTo, ModPlayer> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModRarity<TAttachTo> : ModRarity, IMWModType<TAttachTo, ModRarity> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MWModSceneEffect<TAttachTo> : ModSceneEffect, IMWModType<TAttachTo, ModSceneEffect> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MWModSurfaceBackgroundStyle<TAttachTo> : ModSurfaceBackgroundStyle, IMWModType<TAttachTo, ModSurfaceBackgroundStyle> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MWModUndergroundBackgroundStyle<TAttachTo> : ModUndergroundBackgroundStyle, IMWModType<TAttachTo, ModUndergroundBackgroundStyle> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MWModWaterfallStyle<TAttachTo> : ModWaterfallStyle, IMWModType<TAttachTo, ModWaterfallStyle> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MWModWaterStyle<TAttachTo> : ModWaterStyle, IMWModType<TAttachTo, ModWaterStyle> where TAttachTo : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }

    //weakAttach ↓↓↓↓↓
    public abstract class MWModSystem : ModSystem, IMWWeakModType<ModSystem> { public abstract string[] AttachTo { get; } }
    public abstract class MWModBackgroundStyle : ModBackgroundStyle, IMWWeakModType<ModBackgroundStyle> { public abstract string[] AttachTo { get; } }
    public abstract class MWModModBiome : ModBiome, IMWWeakModType<ModBiome> { public abstract string[] AttachTo { get; } }
    //public abstract class MWModGore : ModGore, IMWWeakModType<ModGore> { public abstract string[] AttachTo { get; } }
    //public abstract class MWModPlayer : ModPlayer, IMWWeakModType<ModPlayer> { public abstract string[] AttachTo { get; } }
    //public abstract class MWModRarity : ModRarity, IMWWeakModType<ModRarity> { public abstract string[] AttachTo { get; } }
    public abstract class MWModSceneEffect : ModSceneEffect, IMWWeakModType<ModSceneEffect> { public abstract string[] AttachTo { get; } }
    public abstract class MWModSurfaceBackgroundStyle : ModSurfaceBackgroundStyle, IMWWeakModType<ModSurfaceBackgroundStyle> { public abstract string[] AttachTo { get; } }
    public abstract class MWModUndergroundBackgroundStyle : ModUndergroundBackgroundStyle, IMWWeakModType<ModUndergroundBackgroundStyle> { public abstract string[] AttachTo { get; } }
    public abstract class MWModWaterfallStyle : ModWaterfallStyle, IMWWeakModType<ModWaterfallStyle> { public abstract string[] AttachTo { get; } }
    public abstract class MWModWaterStyle : ModWaterStyle, IMWWeakModType<ModWaterStyle> { public abstract string[] AttachTo { get; } }

    /*public abstract class MWModGore : ModGore, IMWWeakModType<ModGore> { public abstract string[] AttachTo { get; } }
    public abstract class MWModSystem : ModSystem, IMWWeakModType<ModSystem> { public abstract string[] AttachTo { get; } }
    public abstract class MWModAccessorySlot : ModAccessorySlot, IMWWeakModType<ModAccessorySlot> { public abstract string[] AttachTo { get; } }
    public abstract class MWModBackgroundStyle : ModBackgroundStyle, IMWWeakModType<ModBackgroundStyle> { public abstract string[] AttachTo { get; } }
    public abstract class MWModModBiome : ModBiome, IMWWeakModType<ModBiome> { public abstract string[] AttachTo { get; } }
    public abstract class MWModBlockType : ModBlockType, IMWWeakModType<ModBlockType> { public abstract string[] AttachTo { get; } }
    public abstract class MWModBossBar : ModBossBar, IMWWeakModType<ModBossBar> { public abstract string[] AttachTo { get; } }
    public abstract class MWModBossBarStyle : ModBossBarStyle, IMWWeakModType<ModBossBarStyle> { public abstract string[] AttachTo { get; } }
    public abstract class MWModBuff : ModBuff, IMWWeakModType<ModBuff> { public abstract string[] AttachTo { get; } }
    public abstract class MWModModCactus : ModCactus, IMWWeakModType<ModCactus> { public abstract string[] AttachTo { get; } }
    public abstract class MWModCommand : ModCommand, IMWWeakModType<ModCommand> { public abstract string[] AttachTo { get; } }
    public abstract class MWModDust : ModDust, IMWWeakModType<ModDust> { public abstract string[] AttachTo { get; } }
    [Obsolete("Items as objects that need to be common between worlds cannot be modified", true)]
    public abstract class MWModItem : ModItem, IMWWeakModType<ModItem> { public abstract string[] AttachTo { get; } }
    public abstract class MWModMapLayer : ModMapLayer, IMWWeakModType<ModMapLayer> { public abstract string[] AttachTo { get; } }
    public abstract class MWModMenu : ModMenu, IMWWeakModType<ModMenu> { public abstract string[] AttachTo { get; } }
    public abstract class MWModMount : ModMount, IMWWeakModType<ModMount> { public abstract string[] AttachTo { get; } }
    public abstract class MWModNPC : ModNPC, IMWWeakModType<ModNPC> { public abstract string[] AttachTo { get; } }
    public abstract class MWModPalmTree : ModPalmTree, IMWWeakModType<ModPalmTree> { public abstract string[] AttachTo { get; } }
    public abstract class MWModPlayer : ModPlayer, IMWWeakModType<ModPlayer> { public abstract string[] AttachTo { get; } }
    public abstract class MWModPrefix : ModPrefix, IMWWeakModType<ModPrefix> { public abstract string[] AttachTo { get; } }
    public abstract class MWModProjectile : ModProjectile, IMWWeakModType<ModProjectile> { public abstract string[] AttachTo { get; } }
    public abstract class MWModPylon : ModPylon, IMWWeakModType<ModPylon> { public abstract string[] AttachTo { get; } }
    public abstract class MWModRarity : ModRarity, IMWWeakModType<ModRarity> { public abstract string[] AttachTo { get; } }
    public abstract class MWModSceneEffect : ModSceneEffect, IMWWeakModType<ModSceneEffect> { public abstract string[] AttachTo { get; } }
    public abstract class MWModSurfaceBackgroundStyle : ModSurfaceBackgroundStyle, IMWWeakModType<ModSurfaceBackgroundStyle> { public abstract string[] AttachTo { get; } }
    public abstract class MWModTexturedType : ModTexturedType, IMWWeakModType<ModTexturedType> { public abstract string[] AttachTo { get; } }
    public abstract class MWModTile : ModTile, IMWWeakModType<ModTile> { public abstract string[] AttachTo { get; } }
    public abstract class MWModTileEntity : ModTileEntity, IMWWeakModType<ModTileEntity> { public abstract string[] AttachTo { get; } }
    public abstract class MWModTree : ModTree, IMWWeakModType<ModTree> { public abstract string[] AttachTo { get; } }
    public abstract class MWModUndergroundBackgroundStyle : ModUndergroundBackgroundStyle, IMWWeakModType<ModUndergroundBackgroundStyle> { public abstract string[] AttachTo { get; } }
    public abstract class MWModWall : ModWall, IMWWeakModType<ModWall> { public abstract string[] AttachTo { get; } }
    public abstract class MWModWaterfallStyle : ModWaterfallStyle, IMWWeakModType<ModWaterfallStyle> { public abstract string[] AttachTo { get; } }
    public abstract class MWModWaterStyle : ModWaterStyle, IMWWeakModType<ModWaterStyle> { public abstract string[] AttachTo { get; } }*/
}

using MultiWorldLib.Entities;
using Terraria.ModLoader;

namespace MultiWorldLib.ModTypes
{
    //public abstract class MWModAccessorySlot<T> : ModAccessorySlot, IMWModType<T, ModAccessorySlot> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModBlockType<T> : ModBlockType, IMWModType<T, ModBlockType> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //[Obsolete("Objects that need to be common between worlds cannot be modified.", true)]
    //public abstract class MWModBossBar<T> : ModBossBar, IMWModType<T, ModBossBar> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModBossBarStyle<T> : ModBossBarStyle, IMWModType<T, ModBossBarStyle> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //[Obsolete("Objects that need to be common between worlds cannot be modified.", true)]
    //public abstract class MWModBuff<T> : ModBuff, IMWModType<T, ModBuff> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModModCactus<T> : ModCactus, IMWModType<T, ModCactus> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //[Obsolete("Objects that need to be common between worlds cannot be modified.", true)]
    //public abstract class MWModCommand<T> : ModCommand, IMWModType<T, ModCommand> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //[Obsolete("Objects that need to be common between worlds cannot be modified.", true)]
    //public abstract class MWModDust<T> : ModDust, IMWModType<T, ModDust> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //[Obsolete("Objects that need to be common between worlds cannot be modified.", true)]
    //public abstract class MWModItem<T> : ModItem, IMWModType<T, ModItem> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModMapLayer<T> : ModMapLayer, IMWModType<T, ModMapLayer> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModMenu<T> : ModMenu, IMWModType<T, ModMenu> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModMount<T> : ModMount, IMWModType<T, ModMount> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModNPC<T> : ModNPC, IMWModType<T, ModNPC> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModPalmTree<T> : ModPalmTree, IMWModType<T, ModPalmTree> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModPrefix<T> : ModPrefix, IMWModType<T, ModPrefix> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModProjectile<T> : ModProjectile, IMWModType<T, ModProjectile> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModPylon<T> : ModPylon, IMWModType<T, ModPylon> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModTexturedType<T> : ModTexturedType, IMWModType<T, ModTexturedType> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModTile<T> : ModTile, IMWModType<T, ModTile> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModTileEntity<T> : ModTileEntity, IMWModType<T, ModTileEntity> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModWall<T> : ModWall, IMWModType<T, ModWall> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModTree<T> : ModTree, IMWModType<T, ModTree> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MWModSystem<T> : ModSystem, IMWModType<T, ModSystem> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MWModBackgroundStyle<T> : ModBackgroundStyle, IMWModType<T, ModBackgroundStyle> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MWModModBiome<T> : ModBiome, IMWModType<T, ModBiome> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModGore<T> : ModGore, IMWModType<T, ModGore> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModPlayer<T> : ModPlayer, IMWModType<T, ModPlayer> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    //public abstract class MWModRarity<T> : ModRarity, IMWModType<T, ModRarity> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MWModSceneEffect<T> : ModSceneEffect, IMWModType<T, ModSceneEffect> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MWModSurfaceBackgroundStyle<T> : ModSurfaceBackgroundStyle, IMWModType<T, ModSurfaceBackgroundStyle> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MWModUndergroundBackgroundStyle<T> : ModUndergroundBackgroundStyle, IMWModType<T, ModUndergroundBackgroundStyle> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MWModWaterfallStyle<T> : ModWaterfallStyle, IMWModType<T, ModWaterfallStyle> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }
    public abstract class MWModWaterStyle<T> : ModWaterStyle, IMWModType<T, ModWaterStyle> where T : BaseMultiWorld { public virtual string[] AlsoAttachTo { get; } }

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

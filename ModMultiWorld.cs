using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using MultiWorldLib.Entities;
using MultiWorldLib.Net;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace MultiWorldLib
{
    public class ModMultiWorld : Mod
    {
        #region
        public const string PARAM_IS_SUBSERVER = "-multiworld";
        public const string PARAM_CLASSNAME = "-multiworld_classname";
        public const string PARAM_ID = "-multiworld_id";
        public const string PARAM_IS_GEN = "-multiworld_gen";

        public const string CONNECTION_KEY = "tModLoader_MultiWorld";
        #endregion

        public static ModMultiWorld Instance { get; private set; }
        public static ILog Log
            => Instance?.Logger;
        public static MWSide WorldSide
            => Instance._worldSide;
        public static Guid Id
            => Instance._id;
        public static Type? CurrentWorldType
            => CurrentWorld?.GetType();
        public static BaseMultiWorld CurrentWorld
            => Instance._currentWorld;

        private MWSide _worldSide;
        public Guid _id;
        public BaseMultiWorld _currentWorld { get; internal set; }
        internal bool _isGenerateWorld = false;
        internal readonly List<MWWorldTypeInfo> _worlds = new();

        public override void Load()
        {
            Instance = this;
            if (!Directory.Exists(MultiWorldAPI.WorldPath))
                Directory.CreateDirectory(MultiWorldAPI.WorldPath);

            if (Main.dedServ)
            {
                /*_worldSide = Main.netMode == NetmodeID.SinglePlayer
                    ? MWSide.LocalHost
                    : MWSide.MainServer;*/
                _worldSide = Program.LaunchParameters.ContainsKey(PARAM_IS_SUBSERVER)
                    ? MWSide.SubServer
                    : MWSide.MainServer;
            }
            else
            {
                _worldSide = MWSide.Client;
            }
            Log.Info($"Running as [{_worldSide}].");

            //DataBridge.Init();

            FindMultiWorldTypes();
            LoadWorldData();
            LoadParams();
            RegisterDefaultContent();
            On.Terraria.Net.Sockets.TcpSocket.Terraria_Net_Sockets_ISocket_AsyncSend += MWNetManager.OnSendBytes;

            Log.Info($"Loaded {MWConfig.Instance.Worlds.Count} world(s).");
            base.Load();
        }
        public override void Unload()
        {
            Instance._currentWorld?.OnUnload();
            Instance._currentWorld = null;
            //_mwPatcher.UnpatchAll();
            Instance = null;
            On.Terraria.Net.Sockets.TcpSocket.Terraria_Net_Sockets_ISocket_AsyncSend -= MWNetManager.OnSendBytes;

            //DataBridge.Dispose();

            if (WorldSide is MWSide.MainServer)
            {
                MultiWorldAPI.LoadedWorlds.ForEach(w => w.Stop());
                MultiWorldAPI.LoadedWorlds.Clear();
            }

            base.Unload();
        }
        private void FindMultiWorldTypes()
        {
            foreach (var mod in ModLoader.Mods)
            {
                var allTypes = AssemblyManager.GetLoadableTypes(mod.Code);
                var worldTypes = allTypes.Where(t => t.BaseType == typeof(BaseMultiWorld));
                worldTypes.ForEach(baseWorldType =>
                {
                    Log.Info($"Find multiworld: {baseWorldType.FullName}");

                    var worldContent = new List<Type>();
                    allTypes.Where(t => t.BaseType?.IsGenericType == true && t.BaseType.GetInterface("IMWModType`1") != null && t.BaseType.GetGenericArguments().Any(t => t == baseWorldType))
                        .ForEach(t =>
                        {
                            if (t.BaseType?.BaseType is { } originModType && originModType.GetMethod("Register", BindingFlags.NonPublic | BindingFlags.Instance) is { } origin)
                            {
                                worldContent.Add(originModType);
                                Log.Info($"- Find world content: {t.FullName}");
                            }
                        });

                    _worlds.Add(new(baseWorldType, mod, worldContent));
                });
            }

        }
        internal void RegisterDefaultContent()
        {
            var multiWorldMods = ModLoader.Mods.Where(mod => AssemblyManager.GetLoadableTypes(mod.Code).Any(t => t.BaseType == typeof(BaseMultiWorld)));
            foreach (var mod in multiWorldMods)
            {
                if (mod.ContentAutoloadingEnabled)
                {
                    Log.Warn($"[{mod.Name}] Not set >ContentAutoloadingEnabled< to false , this may cause some problems.");
                }
                else
                {
                    LoaderUtils.ForEachAndAggregateExceptions((from t in AssemblyManager.GetLoadableTypes(mod.Code)
                                                               where !t.IsMWModType() //ignore multiworld type
                                                               where !t.IsAbstract && !t.ContainsGenericParameters
                                                               where t.IsAssignableTo(typeof(ILoadable))
                                                               where t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.EmptyTypes) != null
                                                               where AutoloadAttribute.GetValue(t).NeedsAutoloading
                                                               select t).OrderBy((Type type) => type.FullName, StringComparer.InvariantCulture), t => AddContent((ILoadable)Activator.CreateInstance(t, nonPublic: true)));
                }
            }
        }
        internal void LoadParams()
        {
            if (Program.LaunchParameters.TryGetValue(PARAM_CLASSNAME, out var currentClass))
            {
                if (!string.IsNullOrEmpty(currentClass))
                {
                    ActiveWorld(currentClass);
                }
                else
                    Log.Warn($"Cannot found world class <{currentClass}>, running as default world.");
            }
            if (Program.LaunchParameters.TryGetValue(PARAM_IS_GEN, out var gen))
            {
                if (gen == "true")
                    _isGenerateWorld = true;
            }
            if (Program.LaunchParameters.TryGetValue(PARAM_IS_GEN, out var id) && Guid.TryParse(id, out var guidId))
            {
                _id = guidId;
            }
            if (Program.LaunchParameters.TryGetValue("evil", out var evil))
            {
                switch (evil)
                {
                    case "0":
                        WorldGen.WorldGenParam_Evil = -1;
                        break;
                    case "1":
                        WorldGen.WorldGenParam_Evil = 0;
                        break;
                    case "2":
                        WorldGen.WorldGenParam_Evil = 1;
                        break;
                }
            }
        }
        internal static void ActiveWorld(string className)
        {
            if (!string.IsNullOrEmpty(className)
                && Instance._worlds.Find(w => w.BaseType.FullName == className) is { } worldInfo)
            {
                var type = worldInfo.BaseType;
                if (WorldSide is MWSide.MainServer || type == CurrentWorldType)
                    return;

                var world = (BaseMultiWorld)Activator.CreateInstance(type);
                if ((world.ActiveSide is ActiveSide.Client && WorldSide is MWSide.SubServer)
                    || (world.ActiveSide is ActiveSide.Server && WorldSide is MWSide.Client))
                {
                    Log.Warn($"Class: {type.FullName} can ONLY load on {world.ActiveSide}");
                    world.OnUnload();
                    return;
                }
                if (CurrentWorld is not null)
                {
                    UnloadWorld(CurrentWorld);
                }
                Instance._currentWorld = world;
                world.ParentMod = worldInfo.ParentMod;
                world.ContentTypes = worldInfo.ModContents;
                world.OnLoad();
                var contents = world.ParentMod.GetContent();
                LoaderUtils.ForEachAndAggregateExceptions(AssemblyManager.GetLoadableTypes(world.ParentMod.Code).Where(t => t.IsMWModType()).OrderBy(type => type.FullName),
                    t =>
                    {
                        if (!contents.Any(c => c.GetType() == t))
                            world.ParentMod.AddContent((ILoadable)Activator.CreateInstance(t, nonPublic: true));
                        else
                            Log.Warn($"Content: [{t.FullName}] has loaded and could not be load again, ignoring.");
                    });

                Log.Info($"World class loaded, running as <{type.FullName}>");
            }
            else
                Log.Warn($"Cannot found world class <{className}>.");
        }
        internal static void LoadWorldData()
        {
            MultiWorldAPI.WorldData.Clear();
            foreach (var world in MWConfig.Instance.Worlds)
            {
                MultiWorldAPI.WorldData.Add(world.GetDataAndClone());
            }
        }
        public static void UnloadWorld(BaseMultiWorld world)
        {
            world.OnUnload();

            var modSystemsByMod = typeof(SystemLoader).GetField("SystemsByMod", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as Dictionary<Mod, List<ModSystem>>;
            if (modSystemsByMod.TryGetValue(world.ParentMod, out var modSystems))
            {
                modSystems.Where(s => world.ContentTypes.Contains(s.GetType())).ToList().ForEach(s =>
                {
                    modSystems.Remove(s);
                    s.Unload();
                });
            }
            var modContents = typeof(Mod).GetField("content", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(world.ParentMod) as IList<ILoadable>;
            modContents.Where(c => world.ContentTypes.Contains(c.GetType())).ToList().ForEach(c =>
            {
                modContents.Remove(c);
                c.Unload();
            });
        }
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            MWNetManager.OnRecievePacket(reader, whoAmI);
        }
    }
    public class ModMultiWorldSystem : ModSystem
    {
        public override bool HijackGetData(ref byte messageType, ref BinaryReader reader, int playerNumber)
        {
            return MWNetManager.OnRecieveVanillaPacket(ref messageType, ref reader, playerNumber);
        }
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight)
        {
            if (ModMultiWorld.WorldSide == MWSide.SubServer && ModMultiWorld.CurrentWorld is not null)
            {
                ModMultiWorld.CurrentWorld.OnWorldGen(tasks, ref totalWeight);
                ModMultiWorld.Log.Info($"Modified worldgen progress.");
            }
        }
        public override void SaveWorldData(TagCompound tag)
        {
            if (ModMultiWorld.Instance._isGenerateWorld)
            {
                Environment.Exit(114514);
            }
            base.SaveWorldData(tag);
        }
    }
}
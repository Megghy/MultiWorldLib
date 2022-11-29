using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using MultiWorldLib.Entities;
using MultiWorldLib.Net;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
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
        public static MWSide WorldSide { get; private set; }

        public static Guid Id { get; private set; }
        public static Type? CurrentWorldType
            => CurrentWorld?.GetType();
        public static BaseMultiWorld CurrentWorld { get; internal set; }

        internal bool _isGenerateWorld = false;

        public override void Load()
        {
            Instance = this;

            if (!Directory.Exists(MultiWorldAPI.WorldPath))
                Directory.CreateDirectory(MultiWorldAPI.WorldPath);

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                WorldSide = MWSide.Client;
            }
            else if (Program.LaunchParameters.ContainsKey(PARAM_IS_SUBSERVER))
            {
                WorldSide = MWSide.SubServer;
                LoadParams();
            }
            else
            {
                WorldSide = MWSide.HostServer;
                LoadWorldData();
                Console.WriteLine($"Loaded {MWConfig.Instance.Worlds.Count} world(s).");
            }
            base.Load();
        }
        internal void LoadParams()
        {
            if (Program.LaunchParameters.TryGetValue(PARAM_CLASSNAME, out var currentClass))
            {
                if (!string.IsNullOrEmpty(currentClass) && Assembly.GetExecutingAssembly().GetType(currentClass) is { } excuteType)
                {
                    ActiveWorld(excuteType);
                }
                else
                    Log.Warn($"Cannot found <{currentClass}>");
            }
            if (Program.LaunchParameters.TryGetValue(PARAM_IS_GEN, out var gen))
            {
                if (gen == "true")
                    _isGenerateWorld = true;
            }
            if (Program.LaunchParameters.TryGetValue(PARAM_IS_GEN, out var id) && Guid.TryParse(id, out var guidId))
            {
                Id = guidId;
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
        internal static void ActiveWorld(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (WorldSide is MWSide.HostServer || type == CurrentWorldType)
                return;

            var world = (BaseMultiWorld)Activator.CreateInstance(type);
            if (CurrentWorld is not null)
            {
                CurrentWorld.OnExit();
                var systems = typeof(SystemLoader).GetField("Systems").GetValue(null) as List<ModSystem>;
                var systemsByMod = typeof(SystemLoader).GetField("SystemsByMod").GetValue(null) as Dictionary<Mod, List<ModSystem>>;
                systems.Remove(CurrentWorld);
                systemsByMod[Instance].Remove(CurrentWorld);
            }
            CurrentWorld = world;
            world.RegisterDirect();

            Log.Info($"World class loaded, running as <{type.FullName}>");
        }
        internal static void LoadWorldData()
        {
            MultiWorldAPI.WorldData.Clear();
            foreach (var world in MWConfig.Instance.Worlds)
            {
                MultiWorldAPI.WorldData.Add(world.GetData());
            }
        }
        public override void Unload()
        {
            Instance = null;
            CurrentWorld.OnExit();
            CurrentWorld = null;

            MultiWorldAPI.LoadedWorlds.ForEach(w => w.Stop());
            MultiWorldAPI.LoadedWorlds.Clear();

            base.Unload();
        }
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            MWPacketManager.OnRecievePacket(reader, whoAmI);
        }
    }
    public class ModMultiWorldSystem : ModSystem
    {
        public override bool HijackGetData(ref byte messageType, ref BinaryReader reader, int playerNumber)
        {
            return MWPacketManager.OnRecieveVanillaPacket(ref messageType, ref reader, playerNumber);
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
    public abstract class DummyMultiWorld : ModSystem
    {
        protected sealed override void Register()
        {
            //Ä¬ÈÏ²»×¢²á
        }
        internal void RegisterDirect()
        {
            base.Register();
        }
    }
}
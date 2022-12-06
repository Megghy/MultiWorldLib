using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using MultiWorld.Net;
using MultiWorldLib.Entities;
using MultiWorldLib.Net;
using Terraria;
using Terraria.Localization;
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

        public const string PIPE_PREFIX = "MultiWorld.Pipes";

        public const string NETMESSAGE_IGNORE = "MultiWorld.IgnoreNetMessageSend";
        public const string DEFAULT_WORLD_NAME = "__DefaultWorld__";
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
        public static string? CurrentWorldFullName
            => CurrentWorld?.FullName;
        public static string? CurrentWorldName
            => CurrentWorld?.Name;
        public static BaseMultiWorld? CurrentWorld
            => Instance._currentWorld;

        internal MWSide _worldSide;
        public Guid _id;
        public BaseMultiWorld _currentWorld { get; internal set; }
        internal bool _isGenerateWorld = false;
        internal readonly List<MWWorldTypeInfo> _worlds = new();
        internal SimplePipeClient _pipeClient;
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
            MultiWorldManager.LoadWorldData();
            LoadParams();
            RegisterDefaultContent();

            switch (WorldSide)
            {
                case MWSide.SubServer:
                    Log.Info($"Start pipeClient for: {Id}");
                    _pipeClient.Start();
                    _pipeClient.RecieveDataEvent += OnSubServerRecieveData;
                    break;
                case MWSide.MainServer:
                    var world = MultiWorldAPI.CreateSubServer<TESTBASEWORLD>("C:\\Users\\MegghyUwU\\Documents\\My Games\\Terraria\\tModLoader\\Worlds\\2692ea8a-314f-4ce8-a114-de31f02b1497.wld");
                    world.Start();
                    On.Terraria.Net.Sockets.TcpSocket.Terraria_Net_Sockets_ISocket_AsyncSend += MWNetManager.OnSendBytes;
                    break;
            }

            Log.Info($"Loaded {MWConfig.Instance.Worlds.Count} world(s).");
            base.Load();
        }
        public override void Unload()
        {
            Instance._currentWorld?.OnUnload();
            Instance._currentWorld = null;
            //_mwPatcher.UnpatchAll();
            Instance = null;
            switch (WorldSide)
            {
                case MWSide.SubServer:
                    _pipeClient.Dispose();
                    _pipeClient.RecieveDataEvent -= OnSubServerRecieveData;
                    break;
                case MWSide.MainServer:
                    MultiWorldAPI._loadedWorlds.ForEach(w => w.Stop());
                    MultiWorldAPI._loadedWorlds.Clear();
                    On.Terraria.Net.Sockets.TcpSocket.Terraria_Net_Sockets_ISocket_AsyncSend -= MWNetManager.OnSendBytes;
                    break;
            }

            base.Unload();
        }
        private void OnSubServerRecieveData(int length, byte[] data)
        {
            MWNetManager.OnRecieveData(Id, length, data);
        }
        private void FindMultiWorldTypes()
        {
            foreach (var mod in ModLoader.Mods)
            {
                var allTypes = AssemblyManager.GetLoadableTypes(mod.Code);
                var worldTypes = allTypes.Where(t => t.IsAssignableTo(typeof(BaseMultiWorld)) && !t.IsAbstract);
                var allWeakContentType = allTypes.Where(t => !t.IsAbstract
                    && t.GetInterfaces() is { Length: > 1 } interfaces
                    && interfaces.Any(i => i.Name == "IMWWeakModType`1")
                    && interfaces.Contains(typeof(ILoadable)))
                    .ToDictionary(t => t, t =>
                    {
                        dynamic obj = (Activator.CreateInstance(t));
                        return (string[])obj.AttachTo;
                    });
                var allContentType = allTypes.Where(t => !t.IsAbstract
                    && t.GetInterfaces() is { Length: > 1 } interfaces
                    && interfaces.Any(i => i.Name == "IMWModType`2")
                    && interfaces.Contains(typeof(ILoadable)))
                    .ToDictionary(t => t, t => t.GetInterface(typeof(IMWModType<,>).Name).GetGenericArguments().First(t => t.BaseType == typeof(BaseMultiWorld)).FullName);
                foreach (var baseWorldType in worldTypes)
                {
                    Log.Info($"Find multiworld: {baseWorldType.FullName}");

                    var worldClassName = baseWorldType.FullName;
                    var worldContent = new List<Type>();
                    foreach (var weakType in allWeakContentType)
                    {
                        if (weakType.Value.Contains(worldClassName))
                        {
                            worldContent.Add(weakType.Key);
                            Log.Info($"- Find weakContent: {weakType.Key.FullName}");
                        }
                    }
                    foreach (var type in allContentType)
                    {
                        if (type.Value == worldClassName)
                        {
                            worldContent.Add(type.Key);
                            Log.Info($"- Find content: {type.Key.FullName}");
                        }
                    }

                    _worlds.Add(new(baseWorldType, mod, worldContent));
                }
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
                    currentClass.Split(',').ForEach(c => MultiWorldManager.ActiveWorld(c));
                }
                else
                    Log.Warn($"Cannot found world class <{currentClass}>, running as default world.");
            }
            if (Program.LaunchParameters.TryGetValue(PARAM_IS_GEN, out var gen))
            {
                if (gen == "true")
                    _isGenerateWorld = true;
            }
            if (Program.LaunchParameters.TryGetValue(PARAM_ID, out var id) && Guid.TryParse(id, out var guidId))
            {
                _id = guidId;
                _pipeClient = new($"{PIPE_PREFIX}.{id}");
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
        public override bool HijackSendData(int whoAmI, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7)
        {
            return MWNetManager.OnSendData(whoAmI, msgType, remoteClient, ignoreClient, text, number, number2, number3, number4, number5, number6, number7);
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
    public sealed class ModMultiWorldPlayer : ModPlayer
    {
        public override void OnEnterWorld(Player player)
        {
            Main.Map.Clear();
            Main.Map.Load();
        }
    }
}
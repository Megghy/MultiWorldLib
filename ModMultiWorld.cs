using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using log4net;
using MultiWorld.Net;
using MultiWorldLib.Entities;
using MultiWorldLib.Exceptions;
using MultiWorldLib.Net;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace MultiWorldLib
{
    public partial class ModMultiWorld : Mod
    {
        #region
        public const string PARAM_IS_SUBSERVER = "-multiworld";
        public const string PARAM_IS_GEN = "-multiworld_gen";
        public const string PARAM_WORLDINFO = "-multiworld_worldinfo";

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
            => Instance._worldInfo.Id;
        public static Type? CurrentWorldType
            => CurrentWorld?.GetType();
        public static string? CurrentWorldFullName
            => CurrentWorld?.FullName;
        public static string? CurrentWorldName
            => CurrentWorld?.Name;
        public static BaseMultiWorld? CurrentWorld
            => Instance._currentWorld;

        internal MWSide _worldSide;
        public BaseMultiWorld _currentWorld { get; internal set; }
        internal bool _isGenerateWorld = false;
        internal readonly List<MWWorldTypeInfo> _worlds = new();
        internal SimplePipeClient _pipeClient;
        internal MWWorldInfo _worldInfo = MWWorldInfo.Default;

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
            //RegisterDefaultContent();

            switch (WorldSide)
            {
                case MWSide.SubServer:
                    Log.Info($"Start pipeClient for: {Id}");
                    _pipeClient.Start();
                    _pipeClient.RecieveDataEvent += OnSubServerRecieveData;
                    On.Terraria.IO.WorldFile.SaveWorld += OnWorldSave;
                    break;
                case MWSide.MainServer:
                    var world = MultiWorldAPI.CreateSubServer<TESTBASEWORLD>("C:\\Users\\MegghyUwU\\Documents\\My Games\\Terraria\\tModLoader\\Worlds\\2692ea8a-314f-4ce8-a114-de31f02b1497.wld");
                    world.Start();
                    On.Terraria.Net.Sockets.TcpSocket.Terraria_Net_Sockets_ISocket_AsyncSend += MultiWorldNetManager.OnSendBytes;

                    break;
            }
            MultiWorldManager.ActiveWorld(typeof(TESTBASEWORLD).FullName);
            MultiWorldManager.UnloadWorld(CurrentWorld);

            Log.Info($"Loaded {MultiWorldConfig.Instance.Worlds.Count} world(s).");
            base.Load();
        }

        private void OnWorldSave(On.Terraria.IO.WorldFile.orig_SaveWorld orig)
        {
            throw new NotImplementedException();
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
                    On.Terraria.IO.WorldFile.SaveWorld -= OnWorldSave;
                    break;
                case MWSide.MainServer:
                    MultiWorldAPI._loadedWorlds.ForEach(w => w.Stop());
                    MultiWorldAPI._loadedWorlds.Clear();
                    On.Terraria.Net.Sockets.TcpSocket.Terraria_Net_Sockets_ISocket_AsyncSend -= MultiWorldNetManager.OnSendBytes;
                    break;
            }

            base.Unload();
        }
        private void OnSubServerRecieveData(int length, byte[] data)
        {
            MultiWorldNetManager.OnRecieveData(Id, length, data);
        }
        [Obsolete]
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
                    .ToDictionary(t => t, t =>
                    {
                        dynamic obj = (Activator.CreateInstance(t));
                        var list = ((string[])obj.AlsoAttachTo)?.ToList() ?? new();
                        if (!list.Contains(t.BaseType.GenericTypeArguments.First().FullName))
                            list.Add(t.BaseType.GenericTypeArguments.First().FullName);
                        return list.ToArray();
                    });
                foreach (var baseWorldType in worldTypes)
                {
                    Log.Info($"Find multiworld: {baseWorldType.FullName}");

                    var worldClassName = baseWorldType.FullName;
                    var worldContent = new Dictionary<Type, string[]>();
                    foreach (var weakType in allWeakContentType)
                    {
                        if (weakType.Value.Contains(worldClassName) || weakType.Value.Contains("*"))
                        {
                            worldContent.Add(weakType.Key, weakType.Value);
                            Log.Info($"- Find weakContent: {weakType.Key.FullName} {(weakType.Value.Contains("*") ? "<TOALL>" : "")}");
                        }
                    }
                    foreach (var type in allContentType)
                    {
                        if (type.Value.Contains(worldClassName) || type.Value.Contains("*"))
                        {
                            worldContent.Add(type.Key, type.Value);
                            Log.Info($"- Find content: {type.Key.FullName} {(type.Value.Contains("*") ? "<TOALL>" : "")}");
                        }
                    }

                    _worlds.Add(new(baseWorldType, mod, worldContent));
                }
            }

        }
        [Obsolete]
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
                                                               select t).OrderBy((Type type) => type.FullName, StringComparer.InvariantCulture), t => mod.AddContent((ILoadable)Activator.CreateInstance(t, nonPublic: true)));

                }
            }
        }
        internal void LoadParams()
        {
            if (Program.LaunchParameters.TryGetValue(PARAM_WORLDINFO, out var infoBytesString)
                && Utils.TryDeserializeJson<MWWorldInfo>(Encoding.UTF8.GetString(Convert.FromBase64String(infoBytesString)), out var info))
            {
                if (!string.IsNullOrEmpty(info.LoadClass))
                {
                    MultiWorldManager.ActiveWorld(info.LoadClass);
                }
                else
                    Log.Warn($"Invalid world: <{info.LoadClass}>, running as default world.");
                _worldInfo = info;
                _pipeClient = new($"{PIPE_PREFIX}.{info.Id}");
            }
            if (Program.LaunchParameters.TryGetValue(PARAM_IS_GEN, out var gen))
            {
                if (gen == "true")
                    _isGenerateWorld = true;
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
            MultiWorldNetManager.OnRecievePacket(reader, whoAmI);
        }
    }
    /// <summary>
    /// ModCall
    /// </summary>
    public partial class ModMultiWorld : Mod
    {
        private static readonly Exception _invalidParam = new($"Invalid params.");
        public override object Call(params object[] args)
        {
            if (args is null)
            {
                throw new ArgumentNullException(nameof(args), "Arguments cannot be null!");
            }
            if (args.Length == 0)
            {
                throw new ArgumentException("Arguments cannot be empty!");
            }
            switch (args.First().ToString().ToLower())
            {
                case "findbyname":
                    ThrowHelper.CheckSide(MWSide.MainServer);

                    if (args.Length > 1 && args[1] is string name)
                    {
                        if (MultiWorldAPI.TryFindServersByName(name, out var byNameWorlds) && byNameWorlds.Length > 0)
                        {
                            return byNameWorlds.Select(w => w.Id).ToArray();
                        }
                        else
                        {
                            throw new Exception($"Cannot find world with name: {name}");
                        }
                    }
                    else
                        throw _invalidParam;
                case "findbytype":
                    ThrowHelper.CheckSide(MWSide.MainServer);

                    if (args.Length > 1 && args[1] is string or Type)
                    {
                        if (args[1] is string typeName)
                        {
                            if (MultiWorldAPI.TryFindServersByTypeName(typeName, out var byTypeNameWorld))
                            {
                                return byTypeNameWorld.Select(w => w.Id).ToArray();
                            }
                            else
                            {
                                throw new Exception($"Cannot find world with typeName: {typeName}");
                            }
                        }
                        else
                        {
                            if (MultiWorldAPI.TryFindServersByType(args[1] as Type, out var byTypeWorld))
                            {
                                return byTypeWorld.Select(w => w.Id).ToArray();
                            }
                            else
                            {
                                throw new Exception($"Cannot find world with typeName: {args[1]}");
                            }
                        }
                    }
                    else
                        throw _invalidParam;
                case "get":
                    ThrowHelper.CheckSide(MWSide.MainServer);

                    if (args.Length > 1 && args[1] is Guid or string)
                    {
                        var id = args[1] is Guid guid
                            ? guid
                            : Guid.TryParse(args[1] as string, out var stringId)
                                ? stringId
                                : Guid.Empty;
                        if (MultiWorldAPI.TryFindServerById(id, out var byIdWorld))
                        {
                            return byIdWorld;
                        }
                        else
                        {
                            throw new Exception($"Cannot find world with id: {args[1]}");
                        }
                    }
                    else
                        throw _invalidParam;
                case "create":

                    break;
                case "enter":
                    ThrowHelper.CheckSide(MWSide.MainServer);

                    if (args.Length > 1 && args[1] is Guid or )
                    {
                        var id = args[1] is Guid guid
                            ? guid
                            : Guid.TryParse(args[1] as string, out var stringId)
                                ? stringId
                                : Guid.Empty;
                        if (MultiWorldAPI.TryFindServerById(id, out var byIdWorld))
                        {
                            return byIdWorld;
                        }
                        else
                        {
                            throw new Exception($"Cannot find world with id: {args[1]}");
                        }
                    }
                    else
                        throw _invalidParam;
            }
            return null;
        }
        
    }
    public class ModMultiWorldSystem : ModSystem
    {
        public override bool HijackGetData(ref byte messageType, ref BinaryReader reader, int playerNumber)
        {
            return MultiWorldNetManager.OnRecieveVanillaPacket(ref messageType, ref reader, playerNumber);
        }
        public override bool HijackSendData(int whoAmI, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7)
        {
            return MultiWorldNetManager.OnSendData(whoAmI, msgType, remoteClient, ignoreClient, text, number, number2, number3, number4, number5, number6, number7);
        }
        public override void SaveWorldData(TagCompound tag)
        {
            base.SaveWorldData(tag);
            if (ModMultiWorld.Instance._isGenerateWorld)
            {
                Environment.Exit(114514);
            }
        }
        public override void PreSaveAndQuit()
        {
            MultiWorldManager.UnloadWorld(ModMultiWorld.Instance._currentWorld);
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
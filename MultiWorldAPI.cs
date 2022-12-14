using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MultiWorldLib.Entities;
using MultiWorldLib.Exceptions;
using MultiWorldLib.Models;
using MultiWorldLib.Modules;
using MultiWorldLib.Net;
using Terraria;
using Terraria.ID;

namespace MultiWorldLib
{
    public static class MultiWorldAPI
    {
        public readonly static ConcurrentDictionary<int, MWPlayer> Players = new();
        public static IReadOnlyList<MultiWorldContainer> LoadedWorlds
            => _loadedWorlds;
        internal static List<MultiWorldContainer> _loadedWorlds = new();
        public readonly static List<MWWorldInfo> WorldData = new();

        public static MWSide WorldSide
            => ModMultiWorld.Instance._worldSide;
        public static Guid Id
            => ModMultiWorld.Id;
        public static Type? CurrentWorldType
            => CurrentWorld?.GetType();
        public static string? CurrentWorldFullName
            => CurrentWorld?.FullName;
        public static string? CurrentWorldName
            => CurrentWorld?.Name;
        public static BaseMultiWorld? CurrentWorld
            => ModMultiWorld.Instance?._currentWorld;
        public static bool IsDefaultWorld
            => CurrentWorld is null;


        public static string WorldPath
            => Path.Combine(Main.WorldPath, "MultiWorlds");
        public async static Task CreateWorld<T>(
            string name,
            string path = null,
            WorldSize size = WorldSize.Small,
            WorldEnums difficult = WorldEnums.Normal,
            WorldEvil evil = WorldEvil.Corrupt,
            string seed = null
            ) where T : BaseMultiWorld
        {
            await CreateWorldInternal(name, path, size, difficult, evil, seed, typeof(T).FullName);
        }
        public async static Task CreateWorld(
            string multiWorldName,
            string name,
            string path = null,
            WorldSize size = WorldSize.Small,
            WorldEnums difficult = WorldEnums.Normal,
            WorldEvil evil = WorldEvil.Corrupt,
            string seed = null
            )
        {
            await CreateWorldInternal(name, path, size, difficult, evil, seed, multiWorldName);
        }
        public async static Task CreateDefaultWorld(
            string name,
            string path = null,
            WorldSize size = WorldSize.Small,
            WorldEnums difficult = WorldEnums.Normal,
            WorldEvil evil = WorldEvil.Corrupt,
            string seed = null)
        {
            await CreateWorldInternal(name, path, size, difficult, evil, seed);
        }
        private async static Task<MWWorldInfo> CreateWorldInternal(
            string name,
            string path = null,
            WorldSize size = WorldSize.Small,
            WorldEnums difficult = WorldEnums.Normal,
            WorldEvil evil = WorldEvil.Corrupt,
            string seed = null,
            string typeFullName = null)
        {
            ThrowHelper.CheckSide(MWSide.MainServer | MWSide.LocalHost);

            if (!ModMultiWorld.Instance._worlds.Exists(w => w.WorldType.FullName == typeFullName))
                throw new Exception($"Specific world: [{typeFullName}] not exist.");
            name ??= Guid.NewGuid().ToString();
            path ??= Path.Combine(WorldPath, name + ".wld");
            var _process = new Process()
            {
                StartInfo = new()
                {
                    FileName = Environment.ProcessPath,
                    Arguments = $"tModLoader.dll " +
                        $"-server " +
                        $"-port {Utils.GetRandomPort()} " +
                        $"-world \"{path}\" " +
                        $"-autocreate {(byte)size} " +
                        $"-difficulty {(byte)difficult} " +
                        $"-evil {(byte)evil} " +
                        $"-worldname \"{name}\" " +
                        $"-seed \"{seed}\" " +
                        $"{ModMultiWorld.PARAM_IS_SUBSERVER} " +
                        $"{ModMultiWorld.PARAM_IS_GEN} true " +
                        $"{ModMultiWorld.PARAM_WORLDINFO} {new MWWorldInfo() { LoadClass = typeFullName }.SerializeJson()} ",
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                }
            };
            if (Program.LaunchParameters.TryGetValue("-lang", out var lang1))
                _process.StartInfo.Arguments += $"-lang {lang1} ";
            if (Program.LaunchParameters.TryGetValue("-language", out var lang2))
                _process.StartInfo.Arguments += $"-language {lang2} ";
            _process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                Console.WriteLine($"<Subworld Create> {e.Data}");
            };

            if (_process.Start())
            {
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                Task.Run(_process.BeginOutputReadLine);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                await _process.WaitForExitAsync();

                if (_process.ExitCode == 114514)
                {
                    var world = new MWWorldInfo(name, path);
                    MultiWorldConfig.Instance.Worlds.Add(world);
                    MultiWorldConfig.Instance.Save();
                    WorldData.Add(world);

                    ModMultiWorld.Log.Info($"Successfully created world.");
                    return world;
                }
                else
                {
                    ModMultiWorld.Log.Error($"Unexpected exit when create world with code: {_process.ExitCode}. To see more details please check server log file.");
                }
            }
            else
            {
                ModMultiWorld.Log.Error($"Unable to start create world process.");
            }
            _process.Dispose();
            return null;
        }

        public static MultiWorldContainer CreateDefaultSubServer(
            MWWorldInfo config,
            bool addToList = true,
            bool save = false)
        {
            ThrowHelper.CheckSide(MWSide.MainServer | MWSide.LocalHost);

            var container = MultiWorldContainer.CreateDefault(config, save: save);
            if (addToList)
                _loadedWorlds.Add(container);
            return container;
        }
        public static MultiWorldContainer CreateDefaultSubServer(
            string worldPath,
            bool addToList = true,
            bool save = false)
            => CreateDefaultSubServer(Path.GetFileNameWithoutExtension(worldPath), worldPath, addToList, save);
        public static MultiWorldContainer CreateDefaultSubServer(string name,
            string worldPath,
            bool addToList = true,
            bool save = false)
        {
            ThrowHelper.CheckSide(MWSide.MainServer | MWSide.LocalHost);

            return CreateDefaultSubServer(new MWWorldInfo(name, worldPath), save);
        }
        /// <summary>
        /// Create a subserver, Will not start by default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="config"></param>
        /// <param name="createNewIfExist"></param>
        /// <param name="addToList"></param>
        /// <returns></returns>
        public static MultiWorldContainer CreateSubServer<T>(MWWorldInfo config,
            bool addToList = true,
            bool save = false) where T : BaseMultiWorld
        {
            ThrowHelper.CheckSide(MWSide.MainServer | MWSide.LocalHost);

            var container = MultiWorldContainer.Create<T>(config, save: save);
            if (addToList)
                _loadedWorlds.Add(container);
            return container;
        }
        public static MultiWorldContainer CreateSubServer<T>(
            string name,
            string worldPath,
            bool addToList = true,
            bool save = false) where T : BaseMultiWorld
            => CreateSubServer<T>(new MWWorldInfo(name, worldPath), addToList, save);
        public static MultiWorldContainer CreateSubServer<T>(string worldPath, bool addToList = true, bool save = false) where T : BaseMultiWorld
            => CreateSubServer<T>(new MWWorldInfo(Path.GetFileNameWithoutExtension(worldPath), worldPath), addToList, save);
        public static MultiWorldContainer CreateSubServer(
            Type type,
            string name,
            string worldPath,
            bool addToList = true,
            bool save = false)
        {
            ThrowHelper.CheckSide(MWSide.MainServer | MWSide.LocalHost);

            var container = new MultiWorldContainer(type, new(name, worldPath), null, save: save);
            if (addToList)
                _loadedWorlds.Add(container);
            return container;
        }
        /// <summary>
        /// Will join the first server that which match T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="plr"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public static async Task<bool> TryEnterWorldAsync<T>(this MWPlayer plr) where T : BaseMultiWorld
        {
            if (LoadedWorlds.FirstOrDefault(s => s.WorldClassType == typeof(T)) is { } container)
            {
                await plr.EnterWorldAsync(container);
                return true;
            }
            return false;
        }
        public static async Task EnterWorldAsync(Player plr, MultiWorldContainer container)
        {
            if (plr.GetMWPlayer() is { } mwPlr)
            {
                await mwPlr.EnterWorldAsync(container);
            }
            else
                throw new Exception("Unable to get MultiWorldModPlayer.");
        }
        public static async Task EnterWorldAsync(byte whoAmI, MultiWorldContainer container)
        {
            await EnterWorldAsync(Main.player[whoAmI], container);
        }
        public static async Task EnterWorldAsync(this MWPlayer plr, MultiWorldContainer container)
        {
            ThrowHelper.CheckSide(MWSide.MainServer | MWSide.LocalHost);

            if (ModMultiWorld.WorldSide is MWSide.Client or MWSide.SubServer)
            {
                ModMultiWorld.Log.Debug($"Trying to switch world on {ModMultiWorld.WorldSide}, can only called on {MWSide.MainServer} and {MWSide.LocalHost}");
                return;
            }
            if (plr.State is >= PlayerState.Switching and < PlayerState.InSubServer)
            {
                ModMultiWorld.Log.Debug($"Unallowed transmission requests for {plr}");
                return;
            }
            if (container.Players.Contains(plr))
            {
                ModMultiWorld.Log.Debug($"[{plr}] already in world {container.WorldInfo.Name}");
                return;
            }

            if (MultiWorldHooks.OnPreSwitch(plr, container, out _))
                return;
            ModMultiWorld.Log.Info($"Switching [{plr}: {plr.WhoAmI}] to the world: [{container.WorldInfo.Name} : {container.WorldInfo.WorldFilePath}]");

            plr._subPlayerInfo = null;
            plr.State = PlayerState.Switching;
            var adapter = new MWMainAdapter(plr, container); ;
            plr._tempAdapter = adapter;

            MultiWorldNetManager.OnCallEvent(MWEventTypes.PreSwtich, plr); //通知客户端
            var setClassPacket = MultiWorldNetManager.GetMWPacket(MWPacketTypes.SetClientWorldClass);
            setClassPacket.Write((container.WorldInfo with { WorldFilePath = null }).SerializeJson());
            plr.SendPakcetToClientPlayer(setClassPacket);

            if (!container.IsRunning)
                container.Start();

            if (ModMultiWorld.WorldSide is MWSide.LocalHost)
            {
                await EnterWorldInternalLocalPlay(plr, adapter, container);
            }
            else
            {
                await EnterWorldInternalMultiPlay(plr, adapter, container);
            }
        }

        #region
        private static async Task EnterWorldInternalMultiPlay(MWPlayer plr, MWMainAdapter adapter, MultiWorldContainer container)
        {
            try
            {
                var cancel = new CancellationTokenSource(30 * 1000);
                plr._tempAdapter = adapter;
                await adapter.StartAsync()
                    .WaitAsync(new TimeSpan(0, 0, 30))
                    .ContinueWith(async task =>
                    {
                        await adapter.TryConnectAsync();

                        PostPlayerEntered(plr, adapter, container);
                    });
            }
            catch (Exception ex)
            {
                adapter.Dispose();
                plr.State = PlayerState.InMainServer;
                if (ex is SocketException se && se.SocketErrorCode == SocketError.OperationAborted)
                    return;
            }
        }
        private static async Task EnterWorldInternalLocalPlay(MWPlayer plr, MWMainAdapter adapter, MultiWorldContainer container)
        {
            var cancel = new CancellationTokenSource(30 * 1000);
            plr._tempAdapter = adapter;
            await adapter.StartAsync();

            Main.SwitchNetMode(NetmodeID.MultiplayerClient);
            NetMessage.buffer[256] = new()
            {
                whoAmI = 256
            };

            await adapter.TryConnectAsync();

            PostPlayerEntered(plr, adapter, container);
        }
        private static void PostPlayerEntered(MWPlayer plr, MWMainAdapter adapter, MultiWorldContainer container)
        {
            if (ModMultiWorld.WorldSide is not MWSide.MainServer)
                return;
            plr.WorldAdapter = adapter;
            plr._tempAdapter = null;
            plr.World = container;
            container.Players.Add(plr);

            ModMultiWorld.Log.Info($"[{plr.Player.name}] Joined world: {container.WorldInfo.Name}");
            MultiWorldHooks.OnPostSwitch(plr, container, out _);
            MultiWorldNetManager.OnCallEvent(MWEventTypes.PostSwitch, plr); //通知客户端

            #region 设置host服务器中玩家状态
            plr.Player.active = false; //设为未活动
            NetMessage.SendData(MessageID.PlayerActive, -1, plr.WhoAmI, null, plr.WhoAmI, false.GetHashCode()); //隐藏原服务器中的玩家
            #endregion
        }
        #endregion
        public static void BackToMainServer(
            Player plr,
            bool keepInventory = true,
            bool rememberLastPos = true
            )
        {
            if (plr.GetMWPlayer() is { } mwPlr)
            {
                mwPlr.BackToMainServer(keepInventory, rememberLastPos);
            }
            else
                throw new Exception("Unable to get MultiWorldModPlayer.");
        }
        public static void BackToMainServer(
            byte whoAmI,
            bool keepInventory = true,
            bool rememberLastPos = true
            )
        {
            BackToMainServer(Main.player[whoAmI], keepInventory, rememberLastPos);
        }
        public static void BackToMainServer(
            this MWPlayer plr,
            bool keepInventory = true,
            bool rememberLastPos = true
            )
        {
            if (plr is null)
                throw new ArgumentNullException(nameof(plr));
            switch (ModMultiWorld.WorldSide)
            {
                case MWSide.Client:
                    var data = new RawDataBuilder(MessageID.Unused15)
                            .PackByte((byte)MWCustomTypes.RequestBackToMainServer)
                            .GetByteData();
                    Netplay.Connection.Socket.AsyncSend(data, 0, data.Length, Netplay.Connection.ClientWriteCallBack);
                    return;
                case MWSide.SubServer:
                    plr.WorldAdapter.SendToClient(new RawDataBuilder(MessageID.Unused15)
                            .PackByte((byte)MWCustomTypes.RequestBackToMainServer)
                            .GetByteData());
                    return;
            }
            if (plr.State is PlayerState.InMainServer)
            {
                ModMultiWorld.Log.Debug($"[{plr}] already in main world.");
                return;
            }
            if (ModMultiWorld.WorldSide is MWSide.MainServer or MWSide.LocalHost)
            {
                ModMultiWorld.Log.Info($"Send player [{plr.Player.name}] back to main world.");
                try
                {
                    PreSwitchBack(plr, keepInventory);

                    Netplay.Clients[plr.WhoAmI].State = 1;
                    //MWNetManager._syncModMethod.Invoke(null, new object[] { plr.WhoAmI });
                    NetMessage.SendData(MessageID.PlayerInfo, plr.WhoAmI);

                    if (ModMultiWorld.WorldSide is MWSide.LocalHost)
                    {
                        NetMessage.buffer[256].Reset();
                        Main.autoPass = true;
                        try
                        {
                            Netplay.Connection.Socket.Close();
                        }
                        catch { }
                    }
                }
                catch (Exception ex)
                {
                    ModMultiWorld.Log.Error($"Back to host error: {ex}");
                }
            }
        }
        internal static void PreSwitchBack(MWPlayer plr, bool keepInv)
        {
            MultiWorldHooks.OnPlayerBackToHost(plr, out _);
            MultiWorldHooks.OnPreSwitch(plr, null, out _);
            MultiWorldNetManager.OnCallEvent(MWEventTypes.PreSwtich, plr);
            var setClassPacket = MultiWorldNetManager.GetMWPacket(MWPacketTypes.SetClientWorldClass);
            setClassPacket.Write("");
            plr.SendPakcetToClientPlayer(setClassPacket);

            plr.World?.Players.Remove(plr);

            plr._subPlayerInfo = null;
            plr.State = PlayerState.Switching;
            plr.IsSwitchingBack = true;
            plr.IgnoreSyncInventoryPacket = !keepInv;
            var oldPos = plr.Player.position;

            plr.WorldAdapter?.Dispose();
            plr.WorldAdapter = null;
            plr.World = null;
        }
        internal static void PostSwitchBack(MWPlayer plr)
        {
            MultiWorldHooks.OnPostSwitch(plr, null, out _);
            MultiWorldNetManager.OnCallEvent(MWEventTypes.PostSwitch, plr);
            plr.IsSwitchingBack = false;
            plr.IgnoreSyncInventoryPacket = false;
            plr.State = PlayerState.InMainServer;
            ModMultiWorld.Log.Info($"[{plr.Name} Back to main world.]");
        }
        #region Find

        public static bool TryFindFirstServerByT<T>(out MultiWorldContainer[] container)
        {
            return TryFindServersByType(typeof(T), out container);
        }
        public static bool TryFindServersByType(Type type, out MultiWorldContainer[] container)
        {
            container = null;
            ThrowHelper.CheckSide(MWSide.MainServer | MWSide.LocalHost);
            if (LoadedWorlds.Where(s => s.WorldClassType == type).ToArray() is { Length: > 0 } result)
            {
                container = result;
                return true;
            }
            return false;
        }
        public static bool TryFindServerById(Guid id, out MultiWorldContainer container)
        {
            container = null;
            ThrowHelper.CheckSide(MWSide.MainServer | MWSide.LocalHost);
            if (LoadedWorlds.FirstOrDefault(s => s.Id == id) is { } result)
            {
                container = result;
                return true;
            }
            return false;
        }
        public static bool TryFindServersByName(string name, out MultiWorldContainer[] container)
        {
            container = null;
            ThrowHelper.CheckSide(MWSide.MainServer | MWSide.LocalHost);
            if (LoadedWorlds.Where(s => s.WorldInfo.Name == name || s.WorldInfo.Alias == name).ToArray() is { Length: > 0 } result)
            {
                container = result;
                return true;
            }
            return false;
        }
        public static bool TryFindServersByTypeName(string name, out MultiWorldContainer[] container)
        {
            container = null;
            ThrowHelper.CheckSide(MWSide.MainServer | MWSide.LocalHost);
            if (LoadedWorlds.Where(s => s.WorldClassType.FullName == name).ToArray() is { Length: > 0 } result)
            {
                container = result;
                return true;
            }
            return false;
        }
        #endregion
    }
}

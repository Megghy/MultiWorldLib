using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using MultiWorldLib.Entities;
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
        public readonly static List<MWContainer> LoadedWorlds = new();
        public readonly static List<MWWorldInfo> WorldData = new();


        public static string WorldPath
            => Path.Combine(Main.WorldPath, "MultiWorld");
        public async static Task CreateWorld<T>(
            string path = null,
            WorldSize size = WorldSize.Small,
            WorldEnums difficult = WorldEnums.Normal,
            WorldEvil evil = WorldEvil.Corrupt,
            string seed = null
            ) where T : BaseMultiWorld
        {
            await CreateWorldInternal(path, size, difficult, evil, seed, typeof(T));
        }
        public async static Task CreateDefaultWorld(
            string path = null,
            WorldSize size = WorldSize.Small,
            WorldEnums difficult = WorldEnums.Normal,
            WorldEvil evil = WorldEvil.Corrupt,
            string seed = null)
        {
            await CreateWorldInternal(path, size, difficult, evil, seed);
        }
        private async static Task<MWWorldInfo> CreateWorldInternal(
            string path = null,
            WorldSize size = WorldSize.Small,
            WorldEnums difficult = WorldEnums.Normal,
            WorldEvil evil = WorldEvil.Corrupt,
            string seed = null,
            Type type = null)
        {
            var name = Guid.NewGuid().ToString();
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
                        $"-seed \"{seed ?? new Random().Next(100, 1000000000).ToString()}\" " +
                        $"{ModMultiWorld.PARAM_IS_SUBSERVER} " +
                        $"{ModMultiWorld.PARAM_IS_GEN} true " +
                        $"{ModMultiWorld.PARAM_CLASSNAME} {type?.FullName} ",
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
                    MWConfig.Instance.Worlds.Add(world);
                    MWConfig.Instance.Save();
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

        public static MWContainer CreateDefaultSubServer(MWWorldInfo config, bool addToList = true)
        {
            if (ModMultiWorld.WorldSide is MWSide.Client)
                throw new Exception("Try excute on Client side");
            var container = MWContainer.CreateDefault(config);
            if (addToList)
                LoadedWorlds.Add(container);
            return container;
        }
        /// <summary>
        /// Create a subserver, Will not start by default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="config"></param>
        /// <param name="createNewIfExist"></param>
        /// <param name="addToList"></param>
        /// <returns></returns>
        public static MWContainer CreateSubServer<T>(MWWorldInfo config, bool addToList = true) where T : BaseMultiWorld
        {
            if (ModMultiWorld.WorldSide is MWSide.Client)
                throw new Exception("Try excute on Client side");
            var container = MWContainer.Create<T>(config);
            if (addToList)
                LoadedWorlds.Add(container);
            return container;
        }
        /// <summary>
        /// Will join the first server that which match T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="plr"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public static async Task EnterWorldAsync<T>(this MWPlayer plr) where T : BaseMultiWorld
        {
            if (ModMultiWorld.WorldSide is MWSide.Client)
                throw new Exception("Try excute on Client side");
            var container = LoadedWorlds.Find(s => s.WorldClassType == typeof(T));
            await plr.EnterWorldAsync(container);
        }
        public static async Task EnterWorldAsync(this MWPlayer plr, MWContainer container)
        {
            if (MWHooks.OnPreSwitch(plr, container, out _))
                throw new Exception("Try excute on Client side");
            if (ModMultiWorld.WorldSide != MWSide.HostServer)
            {
                ModMultiWorld.Log.Debug($"Trying to switch world on {ModMultiWorld.WorldSide}, can only called on {MWSide.HostServer}");
                return;
            }
            if (plr.State is >= PlayerState.Switching and < PlayerState.InSubServer)
            {
                ModMultiWorld.Log.Debug($"Unallowed transmission requests for {plr}");
                return;
            }
            if (container.Players.Contains(plr))
            {
                ModMultiWorld.Log.Debug($"[{plr}] already in world {container.WorldConfig.Name}");
                return;
            }

            ModMultiWorld.Log.Info($"Switching [{plr}] to the world: [{container.WorldConfig.Name} : {container.WorldConfig.WorldFilePath}]");

            MWPacketManager.OnCallEvent(MWEventTypes.PreSwtich, plr); //通知客户端

            plr.State = PlayerState.Switching;
            var port = Utils.GetRandomPort();
            var adapter = new MWHostAdapter(port, container);
            try
            {
                await adapter.StartAsync();
                await adapter.TryConnectAsync();

                container.Players.Add(plr);

                MWPacketManager.OnCallEvent(MWEventTypes.PostSwitch, plr); //通知客户端

                #region 设置host服务器中玩家状态
                plr.Player.active = false; //设为未活动
                NetMessage.SendData(MessageID.PlayerActive, -1, plr.Index, null, plr.Index, false.GetHashCode()); //隐藏原服务器中的玩家
                #endregion
            }
            catch (Exception ex)
            {
                adapter.Dispose();
                plr.State = PlayerState.ReadyToSwitch;
                if (ex is SocketException se && se.SocketErrorCode == SocketError.OperationAborted)
                    return;
            }
        }
        public static void BackToMainServer(
            this MWPlayer plr,
            bool keepInventory = true,
            bool rememberLastPos = true
            )
        {
            if (plr is null)
                throw new ArgumentNullException(nameof(plr));
            if (!plr.IsInSubWorld || plr.State is not PlayerState.InSubServer)
                return;
            if (ModMultiWorld.WorldSide is MWSide.HostServer)
            {
                try
                {
                    MWHooks.OnPlayerBackToHost(plr, out _);

                    plr.State = PlayerState.Switching;
                    plr.IsSwitchingBack = true;
                    plr.IgnoreSyncInventoryPacket = !keepInventory;

                    int sectionX = Netplay.GetSectionX(0);
                    int sectionX2 = Netplay.GetSectionX(Main.maxTilesX);
                    int sectionX3 = Netplay.GetSectionX(0);
                    int sectionX4 = Netplay.GetSectionX(Main.maxTilesY);
                    for (int i = sectionX; i <= sectionX2; i++)
                    {
                        for (int j = sectionX3; j <= sectionX4; j++)
                        {
                            Netplay.Clients[plr.Index].TileSections[i, j] = false;
                        }
                    }

                    plr.MWAdapter?.SendToClient(new RawDataBuilder(3)
                        .PackByte((byte)plr.Index)
                        .PackByte((byte)true.GetHashCode())
                        .GetByteData()); //修改玩家slot

                    Main.player.Where(p => p != null && p.whoAmI != plr.Index)
                        .ToList()
                        .ForEach(p =>
                    {
                        NetMessage.SendData(MessageID.PlayerActive, plr.Index, -1, null, p.whoAmI, true.GetHashCode());//显示原服务器玩家 
                        NetMessage.SendData(MessageID.SyncPlayer, plr.Index, -1, null, p.whoAmI); //还原其他玩家信息
                    });
                    Main.npc.ForEach(n => NetMessage.SendData(MessageID.SyncNPC, plr.Index, -1, null, n.whoAmI)); //还原npc数据
                    if (!MWHooks.OnSync(plr, out _))
                    {
                        plr.Player.SyncCharacterInfo(); //客户端将会同步本地数据
                    }
                    plr.Player.Spawn(PlayerSpawnContext.SpawningIntoWorld);
                    if (rememberLastPos)
                    {
                        plr.Player.Teleport(plr.Player.position);
                    }
                    else
                        plr.Player.Teleport(new(Main.spawnTileX * 16, (Main.spawnTileY - 3) * 16));
                    NetMessage.SendData(MessageID.WorldData, plr.Index);  //重置ssc状态/发送世界信息

                    plr.IsSwitchingBack = false;
                    plr.IgnoreSyncInventoryPacket = false;
                }
                catch (Exception ex)
                {
                    ModMultiWorld.Log.Error($"Back to host error: {ex}");
                }
            }
        }

        #region Find

        public static bool TryFindFirstWorldByT<T>(out MWContainer container)
        {
            container = null;
            if (ModMultiWorld.WorldSide is MWSide.Client)
                throw new Exception("Try excute on Client side");
            if (LoadedWorlds.Find(s => s.WorldClassType == typeof(T)) is { } result)
            {
                container = result;
                return true;
            }
            return false;
        }
        public static bool TryFindFirstWorldById(Guid id, out MWContainer container)
        {
            container = null;
            if (ModMultiWorld.WorldSide is MWSide.Client)
                throw new Exception("Try excute on Client side");
            if (LoadedWorlds.Find(s => s.Id == id) is { } result)
            {
                container = result;
                return true;
            }
            return false;
        }
        public static bool TryFindWorldsByName(string name, out MWContainer container)
        {
            container = null;
            if (ModMultiWorld.WorldSide is MWSide.Client)
                throw new Exception("Try excute on Client side");
            if (LoadedWorlds.Find(s => s.WorldConfig.Name == name || s.WorldConfig.Alias == name) is { } result)
            {
                container = result;
                return true;
            }
            return false;
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using MultiWorldLib.Entities;
using MultiWorldLib.Exceptions;
using MultiWorldLib.Net;
using MultiWorldLib.Net.MultiWorld.Net;
using Terraria;

namespace MultiWorldLib.Models
{
    public class MultiWorldContainer : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="config"></param>
        /// <param name="params">Should include '-'</param>
        /// <returns></returns>
        public static MultiWorldContainer Create<T>(MWWorldInfo config, int? port = null, bool save = true, Dictionary<string, string> @params = null) where T : BaseMultiWorld
            => new(typeof(T), config, port, save, @params);
        public static MultiWorldContainer CreateDefault(MWWorldInfo config, int? port = null, bool save = true, Dictionary<string, string> @params = null)
            => new(null, config, port, save, @params);
        internal MultiWorldContainer(Type type, MWWorldInfo config, int? port = null, bool save = true, Dictionary<string, string> @params = null)
        {
            ThrowHelper.CheckSide(MWSide.MainServer | MWSide.LocalHost);
            if (!type.IsAssignableTo(typeof(BaseMultiWorld)))
                throw new ArgumentOutOfRangeException(nameof(type));

            port ??= Utils.GetRandomPort();

            WorldClassType = type;
            StartupParams = @params;
            Port = port.Value;
            config.LoadClass = type?.FullName;
            WorldInfo = config;

            _process = new()
            {
                StartInfo = new()
                {
                    FileName = Environment.ProcessPath,
                    Arguments = $"tModLoader.dll " +
                        $"-server " +
                        $"-world \"{config.WorldFilePath}\" " +
                        $"-port {port.Value} " +
                        $"{ModMultiWorld.PARAM_IS_SUBSERVER} " +
                        $"{ModMultiWorld.PARAM_WORLDINFO} \"{Convert.ToBase64String(Encoding.UTF8.GetBytes(config.SerializeJson()))}\" ",
                    //CreateNoWindow = true,
                }
            };
            if (Program.LaunchParameters.TryGetValue("-lang", out var lang1))
                _process.StartInfo.Arguments += $"-lang {lang1} ";
            if (Program.LaunchParameters.TryGetValue("-language", out var lang2))
                _process.StartInfo.Arguments += $"-language {lang2} ";
            if (@params is { Count: > 0 })
                _process.StartInfo.Arguments += string.Join(' ', @params.Select(kv => $"{kv.Key} {kv.Value}"));

            _process.Exited += OnProcessExit;
            _pipeline = new($"{ModMultiWorld.PIPE_PREFIX}.{Id}");
            _pipeline.RecieveDataEvent += RecieveCustomDataInternal;

            if (save)
            {
                MultiWorldConfig.Instance.Worlds.RemoveAll(w => w.Id == config.Id);
                MultiWorldConfig.Instance.Worlds.Add(config);
                MultiWorldConfig.Instance.Save();
            }
        }

        private void RecieveCustomDataInternal(int length, byte[] data)
        {
            MultiWorldNetManager.OnRecieveData(Id, length, data);
        }

        private readonly Process _process;
        private readonly SimplePipeServer _pipeline;

        public Guid Id
            => WorldInfo.Id;
        public int Port { get; init; }
        public bool IsRunning { get; private set; } = false;
        public List<MWPlayer> Players { get; private set; } = new();
        public MWWorldInfo WorldInfo { get; internal set; }
        public Type? WorldClassType { get; init; }
        public IReadOnlyDictionary<string, string> StartupParams { get; init; }

        public void Start()
        {
            if (IsRunning)
                return;
            _process.Start();
            ModMultiWorld.Log.Info($"Start pipeServer for: {Id}");
            _pipeline.StartWithoutWait();

            IsRunning = true;
        }
        public void Stop()
        {
            _process.StandardInput?.WriteLine("exit");
        }
        public void OnProcessExit(object sender, EventArgs e)
        {
            Dispose();
        }
        public void Dispose()
        {
            _process?.StandardInput?.Close();
            _process?.Kill();
            _process?.Dispose();
            _pipeline.RecieveDataEvent -= RecieveCustomDataInternal;
            _pipeline.Dispose();
        }

        #region
        public delegate void OnRecieveData(byte[] data);
        public void SendCustomData(MultiWorldPacket packet)
        {
            SendDataDirect(packet.GetBytes());
        }
        public void SendDataDirect(byte[] data)
        {
            _pipeline.Send(data);
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MultiWorldLib.Entities;
using Terraria;

namespace MultiWorldLib.Models
{
    public class MWContainer : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="config"></param>
        /// <param name="params">Should include '-'</param>
        /// <returns></returns>
        public static MWContainer Create<T>(MWWorldInfo config, int? port = null, Dictionary<string, string> @params = null) where T : BaseMultiWorld
            => new(typeof(T), config, port, @params);
        public static MWContainer CreateDefault(MWWorldInfo config, int? port = null, Dictionary<string, string> @params = null)
            => new(null, config, port, @params);
        internal MWContainer(Type type, MWWorldInfo config, int? port = null, Dictionary<string, string> @params = null)
        {
            port ??= Utils.GetRandomPort();
            _process = new()
            {
                StartInfo = new()
                {
                    FileName = Environment.ProcessPath,
                    Arguments = $"tModLoader.dll " +
                        $"-server " +
                        $"-world \"{config.WorldFilePath}\" " +
                        $"-port \"{port.Value}\" " +
                        $"{ModMultiWorld.PARAM_IS_SUBSERVER} " +
                        $"{ModMultiWorld.PARAM_CLASSNAME} {type.FullName} ",
                    CreateNoWindow = true,
                }
            }; 
            if (Program.LaunchParameters.TryGetValue("-lang", out var lang1))
                _process.StartInfo.Arguments += $"-lang {lang1} ";
            if (Program.LaunchParameters.TryGetValue("-language", out var lang2))
                _process.StartInfo.Arguments += $"-language {lang2} ";
            if (@params is { Count: > 0 })
                _process.StartInfo.Arguments += string.Join(' ', @params.Select(kv => $"{kv.Key} {kv.Value}"));
            
            _process.Exited += OnProcessExit;

            WorldClassType = type;
            Id = Guid.NewGuid();
            StarupParams = @params;
        }

        private readonly Process _process;
        private bool _isRunning = false;

        public Guid Id { get; init; }
        public List<MWPlayer> Players { get; private set; } = new();
        public MWWorldInfo WorldConfig { get; internal set; }
        public Type? WorldClassType { get; init; }
        public IReadOnlyDictionary<string, string> StarupParams { get; init; }

        public void Start()
        {
            if (_isRunning)
                return;
            _process.Start();
            _isRunning = true;
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
            _process?.Kill();
            _process?.Dispose();
        }
    }
}

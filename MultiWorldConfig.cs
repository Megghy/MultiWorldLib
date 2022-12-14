using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using MultiWorldLib.Entities;
using Terraria.ModLoader.Config;

namespace MultiWorldLib
{
    public class MultiWorldConfig
    {
        public static string ConfigPath
            => Path.Combine(ConfigManager.ServerModConfigPath, "MultiWorldConfig.json");
        public static readonly JsonSerializerOptions DefaultSerializerOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        };
        private static MultiWorldConfig _oldInstance = new();
        private static MultiWorldConfig _instance;
        public static MultiWorldConfig Instance { get { _instance ??= Load(); return _instance; } }
        private static bool _first = true;
        public static MultiWorldConfig Load()
        {
            if (File.Exists(ConfigPath))
            {
                try
                {
                    var config = JsonSerializer.Deserialize<MultiWorldConfig>(File.ReadAllText(ConfigPath));
                    _oldInstance = config;
                    if (_first)
                        config.Save();
                    return config;
                }
                catch (Exception ex)
                {
                    if (_first)
                    {
                        ModMultiWorld.Log.Warn($"Unable to load config file.\r\n{ex}");
                        Console.ReadLine();
                    }
                    return _oldInstance;
                }
            }
            else
            {
                var config = new MultiWorldConfig();
                config.Save();
                return config;
            }
        }
        public static void Reload()
        {
            _oldInstance = _instance;
            _instance = null;
            MultiWorldManager.LoadWorldData();
        }
        public void Save()
        {
            if (!Directory.Exists(ConfigManager.ServerModConfigPath))
                Directory.CreateDirectory(ConfigManager.ServerModConfigPath);
            File.WriteAllText(ConfigPath, JsonSerializer.Serialize(this, DefaultSerializerOptions));
        }

        #region cfg
        public bool EnableChatPrefix { get; set; } = true;
        public string ChatPrefixFormat { get; set; } = "[{world.name}] {chat}";
        public bool UpdateWhenNoPlayerInHost { get; set; } = true;
        #endregion

        /// <summary>
        /// Origin data, Under normal circumstances should be use <see cref="MultiWorldAPI.WorldData"/>
        /// </summary>
        public List<MWWorldInfo> Worlds { get; set; } = new();
    }
}

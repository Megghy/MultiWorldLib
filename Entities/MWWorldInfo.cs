using System;
using System.IO;
using System.Text.Json.Serialization;
using Terraria.IO;

namespace MultiWorldLib.Entities
{
    public record MWWorldInfo
    {
        public static readonly MWWorldInfo Default = new()
        {
            LoadClass = ModMultiWorld.DEFAULT_WORLD_NAME,
            Name = ModMultiWorld.DEFAULT_WORLD_NAME,
        };
        public MWWorldInfo(string name, string path)
        {
            Name = name;
            if (!File.Exists(path))
                throw new FileNotFoundException($"World file not exist.", path);
            if (!path.EndsWith(".wld"))
                throw new FileNotFoundException($"Invalid file: {Path.GetFileName(path)}", path);
            WorldFilePath = path;
            Id = Guid.NewGuid();
            Data = GetData();
        }
        public MWWorldInfo()
        {
            Id = Guid.Empty;
        }

        public MWWorldInfo GetDataAndClone()
            => this with { Data = WorldFile.GetAllMetadata(WorldFilePath, false) ?? WorldFileData.FromInvalidWorld(WorldFilePath, false) };
        public WorldFileData GetData()
            => WorldFile.GetAllMetadata(WorldFilePath, false) ?? WorldFileData.FromInvalidWorld(WorldFilePath, false);

        public Guid Id;
        public string Name;
        public string WorldFilePath;
        public string Alias = "";
        public string Color = "B6E0DF";
        public bool Visiable = true;
        public string LoadClass = "";
        public bool AutoLoad = false;

        public int MaxPlayer = 200;

        public MWWorldSetting Settings = new();

        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public WorldFileData Data { get; private set; }
    }
}

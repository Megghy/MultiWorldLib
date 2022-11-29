using System;
using System.IO;
using System.Text.Json.Serialization;
using Terraria.IO;

namespace MultiWorldLib
{
    public record MWWorldInfo
    {
        public MWWorldInfo(string name, string path)
        {
            Name = name;
            if (!File.Exists(path))
                throw new FileNotFoundException($"World file not exist.", path);
            WorldFilePath = path;
            Id = Guid.NewGuid();
            GetData();
        }

        public MWWorldInfo GetData()
            => this with { Data = WorldFile.GetAllMetadata(WorldFilePath, false) ?? WorldFileData.FromInvalidWorld(WorldFilePath, false) };

        public Guid Id;
        public string Name;
        public string WorldFilePath;
        public string Alias = "";
        public string Color = "B6E0DF";
        public bool Visiable = true;

        public int SpawnX = -1;
        public int SpawnY = -1;
        public int MaxPlayer = 200;

        public bool EnableWorldSave = true;
        public bool EnableSaveOnPlayerLeave = true;
        public bool EnableDisposeWhenNonPlayer = true;
        public int DisposeDelaySecond = 60;
        public bool EnableMobSpawn = true;
        public bool EnableAutoLoadOnServerStart = false;

        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public WorldFileData Data { get; private set; }
    }
}

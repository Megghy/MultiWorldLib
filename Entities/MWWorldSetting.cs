namespace MultiWorldLib.Entities
{
    public record MWWorldSetting
    {
        public int SpawnX = -1;
        public int SpawnY = -1;
        public bool EnableWorldSave = true;
        public bool EnableSaveOnPlayerLeave = true;
        public bool EnableDisposeWhenNonPlayer = true;
        public int DisposeDelaySecond = 600;
        public bool EnableMobSpawn = true;
        public bool EnableAutoLoadOnServerStart = false;

        public bool UpdateWhenNonPlayer = true;
    }
}

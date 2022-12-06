namespace MultiWorldLib.Entities
{
    public record MWWorldSetting
    {
        public bool EnableWorldSave = true;
        public bool EnableSaveOnPlayerLeave = true;
        public bool EnableDisposeWhenNonPlayer = true;
        public int DisposeDelaySecond = 60;
        public bool EnableMobSpawn = true;
        public bool EnableAutoLoadOnServerStart = false;

        public bool UpdateWhenNonPlayer = true;
    }
}

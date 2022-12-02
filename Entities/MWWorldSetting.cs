using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

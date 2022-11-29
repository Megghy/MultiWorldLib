using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MultiWorldLib.Interfaces
{
    public interface IMWPlayerInfo
    {
        public int Index { get; set; }
        public int SpawnX { get; set; }
        public int SpawnY { get; set; }
    }
}

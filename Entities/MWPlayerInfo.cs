﻿using Microsoft.Xna.Framework;
using MultiWorldLib.Interfaces;

namespace MultiWorldLib.Entities
{
    public sealed class MWSubPlayerInfo : IMWPlayerInfo
    {
        public MWSubPlayerInfo()
        {
        }
        public int Index { get; set; }
        public int SpawnX { get; set; }
        public int SpawnY { get; set; }
    }
    public sealed class MWHostPlayerInfo : IMWPlayerInfo
    {
        public MWHostPlayerInfo(MWPlayer plr)
        {
            _player = plr;
        }
        private readonly MWPlayer _player;
        public int Index
        {
            get => _player.Index;
            set { }
        }
        public int SpawnX { get => _player.Player.SpawnX; set { } }
        public int SpawnY { get => _player.Player.SpawnY; set { } }
    }
}
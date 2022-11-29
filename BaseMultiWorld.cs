using System;
using System.Collections.Generic;
using MultiWorldLib.Interfaces;
using Terraria.WorldBuilding;

namespace MultiWorldLib
{
    public abstract class BaseMultiWorld : IDisposable
    {
        public BaseMultiWorld() { }
        public abstract IMWClientHandler ClientHandler { get; }
        public abstract IMWServerHandler ServerHandler { get; }
        public virtual void OnWorldGen(List<GenPass> tasks, ref float totalWeight)
        {

        }
        public abstract void Dispose();
    }
}

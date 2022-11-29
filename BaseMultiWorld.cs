using System;
using System.Collections.Generic;
using MultiWorldLib.Entities;
using MultiWorldLib.Interfaces;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace MultiWorldLib
{
    /// <summary>
    /// This show work both on subserver and client
    /// </summary>
    public abstract class BaseMultiWorld : DummyMultiWorld, IDisposable
    {
        public static MWSide Side
            => ModMultiWorld.WorldSide;
        public abstract 
        public BaseMultiWorld() { }

        public virtual void OnLoad()
        {

        }
        public virtual void PostEnter(MWPlayer player)
        {

        }
        public virtual void PreEnter(MWPlayer player)
        {

        }
        public virtual void OnLeave(MWPlayer player)
        {

        }
        public abstract void OnExit();
        public virtual void OnWorldGen(List<GenPass> tasks, ref float totalWeight)
        {

        }
        void IDisposable.Dispose()
            => OnExit();
    }
}

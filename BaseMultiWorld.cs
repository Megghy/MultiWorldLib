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
    public abstract class BaseMultiWorld : IDisposable
    {
        #region Internal Data
        internal Mod ParentMod;
        internal List<Type> ContentTypes = new();
        #endregion

        public static MWSide CurrentSide
            => ModMultiWorld.WorldSide;
        public abstract ActiveSide ActiveSide { get; }
        public BaseMultiWorld() { }

        public abstract void OnLoad();
        public abstract void OnUnload();
        public virtual void PostEnter(int playerNumber)
        {

        }
        public virtual void PreEnter(int playerNumber)
        {

        }
        public virtual void OnLeave(int playerNumber)
        {

        }
        public virtual void OnWorldGen(List<GenPass> tasks, ref float totalWeight)
        {

        }
        void IDisposable.Dispose()
            => OnUnload();
    }
}

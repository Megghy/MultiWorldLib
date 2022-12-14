using System;
using System.Collections.Generic;
using MultiWorldLib.Entities;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using static MultiWorldLib.MultiWorldHooks;

namespace MultiWorldLib
{
    /// <summary>
    /// This show work both on subserver and client
    /// </summary>
    public abstract class BaseMultiWorld
    {
        public BaseMultiWorld()
        {
            CurrentType = GetType();
            Namespace = CurrentType.Namespace;
            FullName = CurrentType.FullName;
        }
        #region Internal Data
        public Type CurrentType { get; private set; }
        public string Namespace { get; private set; }
        public string FullName { get; private set; }
        public Mod ParentMod { get; internal set; }
        internal Dictionary<Type, ILoadable> Content = new();
        #endregion

        public static MWSide CurrentSide
            => ModMultiWorld.WorldSide;
        public virtual ActiveSide ActiveSide { get; } = ActiveSide.Both;
        public abstract string Name { get; }

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
        public virtual void OnRecieveCustomPacket(RecieveCustomPacketEventArgs args)
        {

        }
    }
}

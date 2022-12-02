﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiWorldLib.Interfaces;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using tModPorter;

namespace MultiWorldLib.Net
{
    public static class DataBridge
    {
        internal static readonly Dictionary<string, Type> CustomPackets = new();
        internal static void Init()
        {
            ModLoader.Mods.ForEach(mod =>
            {
                try
                {
                    AssemblyManager.GetLoadableTypes(mod.Code)
                        .Where(t => t.BaseType == typeof(BaseMWPacket))
                        .ForEach(t => RegisterCustomPacket(t));
                }
                catch (Exception ex) { ModMultiWorld.Log.Error(ex); }
            });
        }
        internal static void Dispose()
        {
            CustomPackets.Clear();
        }
        public static void RegisterCustomPacket<T>() where T : BaseMWPacket
        {
            RegisterCustomPacket(typeof(T));
        }
        private static void RegisterCustomPacket(Type type)
        {
            var name = (Activator.CreateInstance(type) as BaseMWPacket).Key;
            if (CustomPackets.ContainsKey(name))
                throw new($"CustomPacket: [{name}] already exist.");
            CustomPackets.Add(name, type);
            ModMultiWorld.Log.Info($"Register custom packet with key: {name}");
        }
    }
}

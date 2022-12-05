using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil.Cil;
using MultiWorldLib.Entities;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using static MultiWorldLib.ModMultiWorld;

namespace MultiWorldLib
{
    internal static class MultiWorldManager
    {
        internal static void UnloadWorld(BaseMultiWorld world)
        {
            world.OnUnload();

            #region Unload content
            var loaders = typeof(LoaderManager).GetField("loadersByType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(null) as Dictionary<Type, ILoader>;
            foreach (var loader in loaders)
            {
                if(loader.Key.BaseType?.GenericTypeArguments?.FirstOrDefault() is { } loaderType)
                {
                    world.Content.Where(c => loaderType.IsAssignableFrom(c.Key))
                        .ForEach(c =>
                        {
                            var dict = typeof(ModTypeLookup<>).MakeGenericType(loaderType)
                                .GetField("dict", BindingFlags.NonPublic | BindingFlags.Static)
                                .GetValue(null) as Dictionary<string, dynamic>;
                            var tieredDict = typeof(ModTypeLookup<>).MakeGenericType(loaderType)
                                .GetField("tieredDict", BindingFlags.NonPublic | BindingFlags.Static)
                                .GetValue(null) as Dictionary<string, Dictionary<string, dynamic>>;

                            var modInstance = c.Value as IModType;
                            Unload(modInstance, c.Key.Name, c.Key.FullName);
                            foreach (string item in LegacyNameAttribute.GetLegacyNamesOfType(c.Key))
                            {
                                Unload(modInstance, item, (modInstance.Mod?.Name ?? "Terraria") + "/" + item);
                            }
                            void Unload(IModType instance, string name, string fullName)
                            {
                                dict.Remove(fullName);
                                if (tieredDict.TryGetValue(((IModType)instance).Mod?.Name ?? "Terraria", out var value))
                                {
                                    value.Remove(name);
                                }
                            }
                        });
                }
            }
            //modsystem
            var modSystemsByMod = typeof(SystemLoader).GetField("SystemsByMod", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as Dictionary<Mod, List<ModSystem>>;
            var modSystems = typeof(SystemLoader).GetField("Systems", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as List<ModSystem>;
            if (modSystemsByMod.TryGetValue(world.ParentMod, out var modSystem))
            {
                modSystem.Where(s => world.Content.ContainsKey(s.GetType()))
                    .ToList()
                    .ForEach(s =>
                    {
                        modSystem.Remove(s);
                        modSystems.Remove(s);
                        s.Unload();
                    });
            }
            #endregion
            var modContents = world.ParentMod.GetContent() as IList<ILoadable>;
            modContents.Where(c => world.Content.ContainsKey(c.GetType()))
                .ToList()
                .ForEach(c =>
                {
                    modContents.Remove(c);
                    c.Unload();
                });
        }

        internal static void ActiveWorld(string className)
        {
            try
            {
                if (className == DEFAULT_WORLD_NAME)
                {
                    
                }
                else if (!string.IsNullOrEmpty(className)
                && Instance._worlds.Find(w => w.WorldType.FullName == className) is { } worldInfo)
                {
                    var type = worldInfo.WorldType;
                    if (WorldSide is MWSide.MainServer || type == CurrentWorldType)
                        return;
                    if (Instance._worlds.Exists(w => w.GetType() == type))
                        Log.Warn($"World: [{type}] has loaded and could not be load again, ignoring.");

                    if (Activator.CreateInstance(type) is not BaseMultiWorld world)
                        throw new Exception($"Unablt to create world instance.");
                    if ((world.ActiveSide is ActiveSide.Client && WorldSide is MWSide.SubServer)
                        || (world.ActiveSide is ActiveSide.Server && WorldSide is MWSide.Client))
                    {
                        Log.Warn($"Class: {type.FullName} can ONLY load on {world.ActiveSide}");
                        UnloadWorld(world);
                        return;
                    }
                    if(Instance._currentWorld is not null)
                    {
                        UnloadWorld(Instance._currentWorld);
                    }
                    Instance._currentWorld = world;
                    Log.Info($"World: [{type.FullName}] loading, loaded world(s): {Instance._worlds.Count}");

                    world.ParentMod = worldInfo.ParentMod;
                    world.OnLoad();
                    var contents = world.ParentMod.GetContent();
                    LoaderUtils.ForEachAndAggregateExceptions(AssemblyManager.GetLoadableTypes(world.ParentMod.Code).Where(t => t.IsMWModType()).OrderBy(type => type.FullName),
                        t =>
                        {
                            if (!contents.Any(c => c.GetType() == t))
                            {
                                var content = (ILoadable)Activator.CreateInstance(t, nonPublic: true);
                                world.ParentMod.AddContent(content);
                                world.Content.Add(t, content);
                                Log.Info($"- Content: [{t.FullName}] loaded.");
                            }
                            else
                                Log.Warn($"- Content: [{t.FullName}] has loaded and could not be load again, ignoring.");
                        });
                }
                else
                    Log.Warn($"Cannot found world: <{className}>.");
            }
            catch (Exception ex)
            {
                Log.Error($"Cannot load world: <{className}>\r\n{ex}");
            }
        }

        internal static void LoadWorldData()
        {
            MultiWorldAPI.WorldData.Clear();
            foreach (var world in MWConfig.Instance.Worlds)
            {
                MultiWorldAPI.WorldData.Add(world.GetDataAndClone());
            }
        }
    }
}

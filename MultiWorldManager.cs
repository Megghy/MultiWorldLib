using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using Microsoft.Xna.Framework.Input;
using MultiWorldLib.Entities;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using static MultiWorldLib.ModMultiWorld;

namespace MultiWorldLib
{
    internal static class MultiWorldManager
    {
        internal static void UnloadWorld(BaseMultiWorld world)
        {
            if (world is null)
                return;
            world.OnUnload();

            #region Unload content
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
            //no need now
            //获取所有注册的loader
            var loaders = typeof(LoaderManager).GetField("loadersByType", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as Dictionary<Type, ILoader>;
            foreach (var content in world.Content)
            {
                var interfaces = content.Key.GetInterfaces();
                Type contentBaseType;
                if (interfaces.FirstOrDefault(i => i.Name == "IMWModType`2") is { } weakType)
                {
                    contentBaseType = weakType.GenericTypeArguments.First(t => t.IsAssignableTo(typeof(ILoadable)));
                }
                else
                {
                    contentBaseType = interfaces.First(i => i.Name == "IMWWeakModType`1").GenericTypeArguments.First();
                }
                if (loaders.TryGetValue(contentBaseType, out var loader))
                {
                    var contentList = content.Key.GetField("list", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(content.Value) as List<dynamic>; //获取Loader<T>中储存的content
                    contentList.Remove(content.Value);
                }
                //获取泛型静态类实例中的content
                dynamic dict = typeof(ModTypeLookup<>).MakeGenericType(contentBaseType)
                    .GetField("dict", BindingFlags.NonPublic | BindingFlags.Static)
                    .GetValue(null);
                dynamic tieredDict = typeof(ModTypeLookup<>).MakeGenericType(contentBaseType)
                    .GetField("tieredDict", BindingFlags.NonPublic | BindingFlags.Static)
                    .GetValue(null);
                if(dict is not null && tieredDict is not null)
                {
                    var modInstance = content.Value as IModType;
                    Unload(modInstance, modInstance.Name, modInstance.FullName);
                    foreach (string item in LegacyNameAttribute.GetLegacyNamesOfType(content.Key))
                    {
                        Unload(modInstance, item, (modInstance.Mod?.Name ?? "Terraria") + "/" + item);
                    }
                    void Unload(IModType instance, string name, string fullName)
                    {
                        dict.Remove(fullName);
                        var key = instance.Mod?.Name ?? "Terraria";
                        if (tieredDict.ContainsKey(key))
                        {
                            tieredDict[key].Remove(name);
                        }
                    }
                }
            }
            var systemHookLists = typeof(SystemLoader).GetFields(BindingFlags.NonPublic | BindingFlags.Static).Where(p => p.FieldType.Name == "HookList");
            foreach (var item in systemHookLists)
            {
                var hookList = item.GetValue(null);
                var tempSystemsField = hookList.GetType().GetField("arr");
                var tempSystems = tempSystemsField.GetValue(hookList) as ModSystem[];
                tempSystemsField.SetValue(hookList, tempSystems.Where(t => !world.Content.ContainsKey(t.GetType())).ToArray());
            }
            //UnloadContentInstance(typeof(ContentInstance<ModBiome>));
            #endregion
            var modContents = world.ParentMod.GetContent() as IList<ILoadable>;
            modContents.Where(c => world.Content.ContainsKey(c.GetType()))
                .ToList()
                .ForEach(c =>
                {
                    modContents.Remove(c);
                });

            world.Content.ForEach(c => c.Value.Unload());
            world.Content.Clear();
            Log.Info($"World: <{CurrentWorld?.FullName}> Unloaded.");
        }
        private static void UnloadContentInstance(Type type)
        {
            type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic)
                .SetValue(null, null);
            var instances = type.GetProperty("Instances", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var types = instances.GetValue(null) as IEnumerable<dynamic>;
            instances.SetValue(null, types.Where(t => t.GetType() != type).ToArray());
        }
        private static readonly FieldInfo _modIsLoadingField = typeof(Mod).GetField("loading", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static void ActiveWorld(string className)
        {
            try
            {
                if (string.IsNullOrEmpty(className) || className == DEFAULT_WORLD_NAME)
                {
                    UnloadWorld(Instance._currentWorld);
                    Instance._currentWorld = null;
                    Instance._worldInfo = MWWorldInfo.Default;
                    Log.Info($"Default world loaded.");
                }
                else if (Instance._worlds.Find(w => w.WorldType.FullName == className) is { } worldInfo)
                {
                    var type = worldInfo.WorldType;
                    if (WorldSide is MWSide.MainServer || type == CurrentWorldType)
                        return;
                    if (Instance._currentWorld?.GetType() == type)
                    {
                        Log.Warn($"World: [{type}] has loaded and could not be load again, ignoring.");
                        return;
                    }

                    if (Activator.CreateInstance(type) is not BaseMultiWorld world)
                        throw new Exception($"Unablt to create world instance.");

                    if ((world.ActiveSide is ActiveSide.Client && WorldSide is MWSide.SubServer)
                        || (world.ActiveSide is ActiveSide.Server && WorldSide is MWSide.Client))
                    {
                        Log.Warn($"World: <{type.FullName}> can ONLY load on {world.ActiveSide}");
                        UnloadWorld(world);
                        return;
                    }
                    if (Instance._currentWorld is not null)
                    {
                        UnloadWorld(Instance._currentWorld);
                    }
                    Instance._currentWorld = world;
                    Log.Info($"World: <{type.FullName}> loading...");

                    world.ParentMod = worldInfo.ParentMod;
                    world.OnLoad();
                    var contents = world.ParentMod.GetContent();
                    LoaderUtils.ForEachAndAggregateExceptions(worldInfo.ModContents.Keys,
                        t =>
                        {
                            if (contents.FirstOrDefault(c => c.GetType() == t) is { } existContent)
                            {
                                Log.Warn($"- Content: [{t.FullName}] has loaded and could not be load again, ignoring.");
                                world.Content.Add(t, existContent);
                            }
                            else
                            {
                                _modIsLoadingField.SetValue(world.ParentMod, true);
                                var content = (ILoadable)Activator.CreateInstance(t, nonPublic: true);
                                world.ParentMod.AddContent(content);
                                world.Content.Add(t, content);
                                _modIsLoadingField.SetValue(world.ParentMod, false);
                                Log.Info($"- Content: [{t.FullName}] loaded.");
                            }
                        });
                    typeof(SystemLoader).GetMethod("RebuildHooks", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
                    Log.Debug($"Rebuild hooks...");
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
            foreach (var world in MultiWorldConfig.Instance.Worlds)
            {
                MultiWorldAPI.WorldData.Add(world.GetDataAndClone());
            }
        }
    }
}

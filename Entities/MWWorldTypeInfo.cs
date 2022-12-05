using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace MultiWorldLib.Entities
{
    public record MWWorldTypeInfo(Type WorldType, Mod ParentMod, List<Type> ModContents);
}

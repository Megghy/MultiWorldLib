using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace MultiWorldLib.Entities
{
    public interface IMWModType<T, TModType> where T : BaseMultiWorld where TModType : ILoadable
    {
        public string[] AlsoAttachTo { get; }
    }
    public interface IMWWeakModType<TModType> where TModType : ILoadable
    {
        public string[] AttachTo { get; }
    }
}

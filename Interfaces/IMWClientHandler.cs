using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWorldLib.Interfaces
{
    public interface IMWClientHandler
    {
        /// <summary>
        /// Called when entering a subworld. Before this is called, the return button and underworld's visibility are reset.
        /// </summary>
        public virtual void OnEnter() { }
        /// <summary>
        /// Called when exiting a subworld. After this is called, the return button and underworld's visibility are reset.
        /// </summary>
        public virtual void OnExit() { }
    }
}

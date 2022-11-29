using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWorldLib.Interfaces
{
    public interface IMWServerHandler
    {
        /// <summary>
        /// Called when entering a subworld. Before this is called, the return button and underworld's visibility are reset.
        /// </summary>
        public virtual void OnEnter() { }
        /// <summary>
        /// Called when exiting a subworld. After this is called, the return button and underworld's visibility are reset.
        /// </summary>
        public virtual void OnExit() { }
        /// <summary>
        /// Called after the subworld generates or loads from file.
        /// </summary>
        public virtual void OnLoad() { }
        /// <summary>
        /// Called while leaving the subworld, before a different world generates or loads from file.
        /// </summary>
        public virtual void OnUnload() { }
    }
}

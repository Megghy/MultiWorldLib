using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWorldLib.Entities
{
    public enum PlayerState
    {
        Disconnect,
        NewConnection,
        InMainServer,
        Switching,
        RequirePassword, 
        SyncData,
        InSubServer,
    }
}

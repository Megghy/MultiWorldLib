using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiWorldLib.Entities;

namespace MultiWorldLib.Exceptions
{
    public class WrongSideException : Exception
    {
        public WrongSideException(MWSide allowSide) : base($"Cannot use this in {ModMultiWorld.WorldSide}, it can only run in {allowSide}.") 
        { }
    }
}

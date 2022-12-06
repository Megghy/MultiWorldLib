using System;
using MultiWorldLib.Entities;

namespace MultiWorldLib.Exceptions
{
    public class WrongSideException : Exception
    {
        public WrongSideException(MWSide allowSide) : base($"Cannot use this in {ModMultiWorld.WorldSide}, it can only run in {allowSide}.")
        { }
    }
}

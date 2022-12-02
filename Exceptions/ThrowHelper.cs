using MultiWorldLib.Entities;

namespace MultiWorldLib.Exceptions
{
    public static class ThrowHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="allowSide"></param>
        /// <exception cref="WrongSideException"></exception>
        public static void CheckSide(MWSide allowSide)
        {
            if(!allowSide.HasFlag(ModMultiWorld.WorldSide))
                throw new WrongSideException(allowSide);
        }
    }
}

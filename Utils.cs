using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using MultiWorldLib.Entities;
using Steamworks;

namespace MultiWorldLib
{
    public static class Utils
    {
        public static int GetRandomPort()
        {
            var random = new Random();
            var randomPort = random.Next(10000, 65535);

            while (IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Any(p => p.Port == randomPort))
            {
                randomPort = random.Next(10000, 65535);
            }

            return randomPort;
        }
        public static void ForEach<T>(this IEnumerable<T> array,  Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            foreach (var item in array)
            {
                action(item);
            }
        }
    }
}

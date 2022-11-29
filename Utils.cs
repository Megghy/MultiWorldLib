using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MultiWorldLib.Entities;
using Steamworks;
using Terraria.ModLoader;
using XPT.Core.Audio.MP3Sharp.Decoding;

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
        public static byte[] ToBytes(this BinaryReader reader)
        {
            return ((MemoryStream)reader.BaseStream).GetBuffer();
        }
        public static byte[] ToBytes(this ModPacket packet)
        {
            var len = (ushort)packet.BaseStream.Position;
            packet.Seek(0, SeekOrigin.Begin);
            packet.Write(len);
            packet.Close();
            return ((MemoryStream)packet.BaseStream).GetBuffer();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using MultiWorldLib.Entities;
using Terraria;
using Terraria.ModLoader;

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
        public static void ForEach<T>(this IEnumerable<T> array, Action<T> action)
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
        public static byte[] ToBytes(this BinaryReader reader, bool keepPos = true)
        {
            var pos = reader.BaseStream.Position;
            reader.BaseStream.Position = 0L;
            var len = reader.ReadInt16();
            reader.BaseStream.Position = 0L;
            var data = reader.ReadBytes(len);
            if (keepPos)
                reader.BaseStream.Position = pos;
            return data;
        }
        public static byte[] ToBytes(this BinaryReader reader, int start, int length, bool keepPos = true)
        {
            var pos = reader.BaseStream.Position;
            reader.BaseStream.Position = start;
            var data = reader.ReadBytes(length);
            if (keepPos)
                reader.BaseStream.Position = pos;
            return data;
        }
        private readonly static FieldInfo _modPacketBuf = typeof(ModPacket).GetField("buf", BindingFlags.NonPublic | BindingFlags.Instance);
        private readonly static MethodInfo _modPacketFinish = typeof(ModPacket).GetMethod("Finish", BindingFlags.NonPublic | BindingFlags.Instance);
        public static byte[] ToBytes(this ModPacket packet)
        {
            _modPacketFinish.Invoke(packet, Array.Empty<object>());
            var data = _modPacketBuf.GetValue(packet) as byte[];
            return data.Take(BitConverter.ToInt16(data, 0)).ToArray();
        }
        public static bool IsMWModType(this Type type)
            => type?.BaseType?.IsGenericType == true
            && type.BaseType.GetInterface("IMWModType`1") != null
            && type.BaseType.GetGenericArguments().Any(t => t.BaseType == typeof(BaseMultiWorld));

        public static Type? GetBaseMultiWorldType(this Type type)
            => type.BaseType?.GetGenericArguments().FirstOrDefault(t => t.BaseType == typeof(BaseMultiWorld));
        public static MWPlayer GetMWPlayer(this Player player)
        {
            var plr = player?.GetModPlayer<MWPlayer>();
            return plr;
        }
        public static MWPlayer GetMWPlayer(this int? playerId)
        {
            if (playerId.HasValue && playerId is >= 0 and < 256)
            {
                return GetMWPlayer(Main.player[playerId.Value]);
            }
            return null;
        }
    }
}

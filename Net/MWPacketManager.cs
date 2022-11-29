using System;
using System.IO;
using MultiWorldLib.Entities;
using MultiWorldLib.Models;
using Terraria;
using Terraria.ModLoader;

namespace MultiWorldLib.Net
{
    public static class MWPacketManager
    {
        public static bool OnRecieveVanillaPacket(ref byte messageType, ref BinaryReader reader, int playerNumber)
        {
            if (ModMultiWorld.WorldSide is not MWSide.Client && Main.player[playerNumber]?.GetModPlayer<MWPlayer>() is { } plr)
            {
                if (ModMultiWorld.WorldSide == MWSide.SubServer && plr.MWAdapter is null)
                {
                    plr.MWAdapter = new MWSubServerAdapter(plr); //如果在子世界中且没有适配器则创建
                }
                return plr.MWAdapter?.OnRecieveVanillaPacket(ref messageType, ref reader) ?? false;
            }
            return false;
        }
        public static void OnRecievePacket(BinaryReader reader, int whoAmI)
        {
            try
            {
                var type = (MWPacketTypes)reader.ReadByte();
                if (ModMultiWorld.WorldSide == MWSide.HostServer)
                {
                    switch (type)
                    {
                    }
                }
                else
                {

                }
            }
            catch (Exception ex) { ModMultiWorld.Log.Error(ex); }
        }
        public static void SendPakcetToClient(this MWPlayer plr, Func<ModPacket, ModPacket> func)
        {
            if (func is null)
                throw new ArgumentNullException(nameof(func));
            if (ModMultiWorld.WorldSide == MWSide.Client)
                throw new Exception("");

            var packet = func(ModMultiWorld.Instance.GetPacket());
            packet.Send();
        }
    }
}

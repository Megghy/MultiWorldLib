using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace MultiWorldLib.Commands
{
    public class MWCommand : ModCommand
    {
        public const string PERM_LIST = "multiworldlib.use.list";
        public const string PERM_TP = "multiworldlib.use.tp";
        public const string PERM_CREATE = "multiworldlib.admin.list";
        public const string PERM_DELETE = "multiworldlib.admin.list";
        public const string PERM_DISPOSE = "multiworldlib.admin.list";

        public override string Command
            => "mw";
        public override CommandType Type
            => CommandType.Console | CommandType.Server;

        /// <summary>
        /// TODO
        /// </summary>
        public static Func<string, bool> CheckPerm 
            => perm => true;

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length > 0)
            {
                switch (args.First().ToLower())
                {
                    case "list":
                        if (CheckPerm(PERM_LIST))
                        {
                            var sb = new StringBuilder();
                            sb.AppendLine($"---------- Worlds -----------");
                            MultiWorldAPI.WorldData.Where(w => w.Visiable).ForEach(world =>
                            {
                                sb.AppendLine($"[C/{world.Color}:{(string.IsNullOrEmpty(world.Alias) ? world.Name : world.Alias)}]");
                            });
                            caller.Reply(sb.ToString());
                        }
                        break;
                    case "b":
                        Main.player[0].GetMWPlayer().BackToMainServer();
                        break;
                }
            }
            else
            {
                Task.Run(() =>
                {
                    var world = MultiWorldAPI.CreateSubServer<TESTBASEWORLD>("C:\\Users\\MegghyUwU\\Documents\\My Games\\Terraria\\tModLoader\\Worlds\\2692ea8a-314f-4ce8-a114-de31f02b1497.wld");
                    Main.player[0].GetMWPlayer().EnterWorldAsync(world);
                });
            }
        }
    }
}

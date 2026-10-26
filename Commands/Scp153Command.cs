using CommandSystem;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace SCP153.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Scp153ParentCommand : ParentCommand
    {
        public Scp153ParentCommand() => LoadGeneratedCommands();

        public override string Command => "scp153";
        public override string[] Aliases => new[] { "153" };
        public override string Description => "Zarzadzanie rola SCP153";

        public sealed override void LoadGeneratedCommands()
        {
            RegisterCommand(new GiveCommand());
            RegisterCommand(new RemoveCommand());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "Uzycie: scp153 (give | remove) [id/nick]";
            return false;
        }

        private class GiveCommand : ICommand
        {
            public string Command => "give";
            public string[] Aliases => new[] { "g" };
            public string Description => "Nadaje role SCP153";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                if (!sender.CheckPermission("scp153.manage"))
                {
                    response = "Brak uprawnien (wymagane: scp153.manage)!";
                    return false;
                }

                if (arguments.Count < 1)
                {
                    response = "Podaj nick lub ID gracza!";
                    return false;
                }

                Player target = Player.Get(arguments.At(0));
                if (target == null)
                {
                    response = "Nie znaleziono gracza.";
                    return false;
                }

                if (CustomRole.TryGet(153, out CustomRole role))
                {
                    role.AddRole(target);
                    response = $"Gracz {target.Nickname} zostal SCP153!";
                    return true;
                }

                response = "Blad: Rola SCP153 nie jest zarejestrowana w systemie.";
                return false;
            }
        }

        private class RemoveCommand : ICommand
        {
            public string Command => "remove";
            public string[] Aliases => new[] { "r" };
            public string Description => "Zdejmuje role SCP153";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                if (!sender.CheckPermission("scp153.manage"))
                {
                    response = "Brak uprawnien (wymagane: scp153.manage)!";
                    return false;
                }

                if (arguments.Count < 1)
                {
                    response = "Podaj nick lub ID gracza!";
                    return false;
                }

                Player target = Player.Get(arguments.At(0));
                if (target == null)
                {
                    response = "Nie znaleziono gracza.";
                    return false;
                }

                if (CustomRole.TryGet(153, out CustomRole role))
                {
                    if (!role.Check(target))
                    {
                        response = $"Gracz {target.Nickname} nie posiada roli SCP153.";
                        return false;
                    }

                    role.RemoveRole(target);
                    response = $"Odebrano role SCP153 graczowi {target.Nickname}";
                    return true;
                }

                response = "Blad roli.";
                return false;
            }
        }
    }
}
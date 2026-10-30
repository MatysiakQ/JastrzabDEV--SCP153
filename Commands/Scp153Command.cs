using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using PlayerRoles;
using System;

namespace SCP153.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public sealed class Scp153ParentCommand : ParentCommand
    {
        public Scp153ParentCommand() => LoadGeneratedCommands();
        public override string Command => "scp153";
        public override string[] Aliases => new[] { "153" };
        public override string Description => "Zarzadzanie rola SCP-153";
        public sealed override void LoadGeneratedCommands()
        {
            RegisterCommand(new GiveCommand());
            RegisterCommand(new RemoveCommand());
        }
        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        { response = "Uzycie: scp153 (give | remove) <id/nick>"; return false; }

        private static bool Resolve(ArraySegment<string> arguments, ICommandSender sender, out Player target, out string response)
        {
            target = null;
            if (!sender.CheckPermission("scp153.manage"))
            { response = "Brak uprawnien: scp153.manage."; return false; }
            if (Plugin.Instance?.IsRunning != true)
            { response = "Plugin SCP-153 nie jest aktywny. Sprawdz log uruchamiania."; return false; }
            if (arguments.Count < 1)
            { response = "Podaj ID lub nick gracza."; return false; }
            target = Player.Get(string.Join(" ", arguments));
            if (target == null)
            { response = "Nie znaleziono gracza."; return false; }
            response = null;
            return true;
        }
        private sealed class GiveCommand : ICommand
        {
            public string Command => "give";
            public string[] Aliases => new[] { "g" };
            public string Description => "Nadaje SCP-153 graczowi bez innej CustomRole";
            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                if (!Resolve(arguments, sender, out var target, out response)) return false;
                return Plugin.Instance.Role.TryGive(target, out response);
            }
        }
        private sealed class RemoveCommand : ICommand
        {
            public string Command => "remove";
            public string[] Aliases => new[] { "r" };
            public string Description => "Usuwa SCP-153 (domyslnie przenosi do Spectator)";
            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                if (!Resolve(arguments, sender, out var target, out response)) return false;
                var role = Plugin.Instance.Role;
                if (!role.Check(target))
                { response = "Gracz nie ma roli SCP-153."; return false; }
                bool kills = role.RemovalKillsPlayer;
                role.RemoveRole(target);
                response = kills && target.Role.Type == RoleTypeId.Spectator
                    ? "Usunieto SCP-153 i przeniesiono gracza do Spectator."
                    : "Usunieto SCP-153; gracz pozostaje w roli bazowej (ustawienie lub blokada zmiany roli).";
                return true;
            }
        }
    }
}

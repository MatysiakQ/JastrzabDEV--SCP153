using Exiled.API.Features;
using Exiled.API.Enums;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using System;
using System.Linq;

namespace SCP153.Roles
{
    public sealed class Scp153 : CustomRole
    {
        private readonly Plugin plugin;
        public override uint Id { get; set; }
        public override string Name { get; set; } = "SCP153";
        public override string Description { get; set; } = "Bulson: ewolucja za skuteczne ugryzienia";
        public override string CustomInfo { get; set; } = "SCP-153 BULSON";
        public override RoleTypeId Role { get; set; } = RoleTypeId.Scp0492;
        public override int MaxHealth { get; set; }
        public override float SpawnChance { get; set; } = 0f;
        public override bool IgnoreSpawnSystem { get; set; } = true;

        public Scp153(Plugin owner)
        {
            plugin = owner;
            Id = owner.Config.CustomRoleId;
            MaxHealth = owner.Config.MaxHealth;
            RemovalKillsPlayer = owner.Config.RemovalKillsPlayer;
        }

        // Avoid CustomRole.AddRole's uncancellable delayed inventory operation.
        // This role has no custom inventory/abilities and owns its initialization explicitly.
        public override void AddRole(Player player)
        {
            if (!TryGive(player, out string reason)) Log.Warn("[SCP-153] " + reason);
        }
        public bool TryGive(Player player, out string reason)
        {
            if (!plugin.IsRunning || !Round.IsStarted || Round.IsEnded)
            { reason = "SCP-153 mozna nadac tylko podczas aktywnej rundy."; return false; }
            if (player?.Connection == null)
            { reason = "Gracz nie jest polaczony."; return false; }
            if (CustomRole.Registered.Any(r => r.Check(player)))
            { reason = "Gracz ma juz CustomRole. Najpierw usun poprzednia role."; return false; }

            var previousRole = player.Role.Base;
            int previousLife = Logic.LogicHandler.CurrentLife(player);
            player.Role.Set(Role, SpawnReason.ForceClass);
            if (player.Role.Type != Role || !player.IsAlive ||
                (ReferenceEquals(previousRole, player.Role.Base) && previousLife == Logic.LogicHandler.CurrentLife(player)))
            { reason = "Zmiana roli bazowej zostala przerwana przez inny system."; return false; }

            TrackedPlayers.Add(player);
            try
            {
                player.MaxHealth = MaxHealth;
                player.Health = MaxHealth;
                RoleAdded(player);
                reason = $"Gracz {player.Nickname} zostal SCP-153.";
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("[SCP-153] Przerwana inicjalizacja: " + ex);
                RemoveOwnedRole(player, true);
                reason = "Blad inicjalizacji SCP-153; sprawdz log serwera.";
                return false;
            }
        }

        protected override void RoleAdded(Player player)
        {
            foreach (var previous in player.GameObject.GetComponents<Scp153Component>())
            {
                previous.Shutdown();
                UnityEngine.Object.Destroy(previous);
            }
            player.GameObject.AddComponent<Scp153Component>().Initialize(player, plugin);
            player.Broadcast(10, "<size=30><color=red><b>JESTES SCP-153 BULSON</b></color></size>\nUgryzienia rozwijaja twoja tarcze i ewolucje.");
        }

        protected override void RoleRemoved(Player player)
        {
            if (player?.GameObject == null) return;
            foreach (var component in player.GameObject.GetComponents<Scp153Component>())
            {
                component.Shutdown();
                UnityEngine.Object.Destroy(component);
            }
        }

        public override void RemoveRole(Player player) => RemoveOwnedRole(player, RemovalKillsPlayer);

        private void RemoveOwnedRole(Player player, bool kill)
        {
            if (!TrackedPlayers.Remove(player)) return;
            RoleRemoved(player);
            // Never force scale/default effects or clear a different plugin's labels.
            if (player?.Connection != null && kill && player.IsAlive)
                player.Role.Set(RoleTypeId.Spectator, SpawnReason.ForceClass);
        }

        // Spawned is a committed change: a cancelled ChangingRole must not remove SCP-153.
        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
            Exiled.Events.Handlers.Player.Destroying += OnDestroying;
        }
        protected override void UnsubscribeEvents()
        {
            DisposePlayers();
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
            Exiled.Events.Handlers.Player.Destroying -= OnDestroying;
        }
        private void OnSpawned(SpawnedEventArgs ev) => RemoveOwnedRole(ev.Player, false);
        private void OnDestroying(DestroyingEventArgs ev) => RemoveOwnedRole(ev.Player, false);

        public void DisposePlayers()
        {
            foreach (var player in TrackedPlayers.ToArray())
            {
                try { RemoveOwnedRole(player, RemovalKillsPlayer); }
                catch (Exception ex)
                {
                    Log.Error("[SCP-153] Blad usuwania wlasnej roli: " + ex);
                    RoleRemoved(player);
                    TrackedPlayers.Remove(player);
                }
            }
        }
    }
}

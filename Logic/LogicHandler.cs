using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCP153.Logic
{
    public static class LogicHandler
    {
        private static readonly Dictionary<Player, int> Lives = new Dictionary<Player, int>();
        private static readonly Dictionary<DamageHandlerBase, PendingHit> Pending = new Dictionary<DamageHandlerBase, PendingHit>();
        private static CoroutineHandle spawnTask;
        private static int round;
        private static int lastPrunedFrame = -1;
        private static bool registered;

        public static void RegisterEvents()
        {
            if (registered) return;
            registered = true;
            Exiled.Events.Handlers.Player.Hurting += OnHurting;
            Exiled.Events.Handlers.Player.Hurt += OnHurt;
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
            Exiled.Events.Handlers.Player.Left += OnLeft;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
            Exiled.Events.Handlers.Server.RestartingRound += ResetRound;
            Exiled.Events.Handlers.Server.WaitingForPlayers += ResetRound;
        }
        public static void UnregisterEvents()
        {
            if (registered)
            {
                registered = false;
                Exiled.Events.Handlers.Player.Hurting -= OnHurting;
                Exiled.Events.Handlers.Player.Hurt -= OnHurt;
                Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
                Exiled.Events.Handlers.Player.Left -= OnLeft;
                Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
                Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
                Exiled.Events.Handlers.Server.RestartingRound -= ResetRound;
                Exiled.Events.Handlers.Server.WaitingForPlayers -= ResetRound;
            }
            ResetRound();
        }

        private static void ResetRound()
        {
            round++;
            Timing.KillCoroutines(spawnTask);
            spawnTask = default;
            Pending.Clear();
            Lives.Clear();
            lastPrunedFrame = -1;
            Plugin.Instance?.Role?.DisposePlayers();
        }
        private static void OnRoundEnded(RoundEndedEventArgs ev) => ResetRound();
        private static void OnRoundStarted()
        {
            Timing.KillCoroutines(spawnTask);
            round++;
            Pending.Clear();
            int expectedRound = round;
            var plugin = Plugin.Instance;
            spawnTask = Timing.CallDelayed(5f, () =>
            {
                if (!registered || plugin == null || !plugin.IsRunning || Plugin.Instance != plugin ||
                    expectedRound != round || !Round.IsStarted || Round.IsEnded) return;
                if (plugin.Config.SpawnChance <= 0f || Random.value >= plugin.Config.SpawnChance) return;
                var pool = Player.List.Where(p => p.IsAlive && p.Role.Team == Team.SCPs &&
                    p.Role.Type != RoleTypeId.Scp079 &&
                    !Exiled.CustomRoles.API.Features.CustomRole.Registered.Any(r => r.Check(p))).ToList();
                if (pool.Count == 0) return;
                var chosen = pool[Random.Range(0, pool.Count)];
                if (!plugin.Role.TryGive(chosen, out string result)) { Log.Warn("[SCP-153] " + result); return; }
                if (!TeleportService.TryRelocate(chosen, true))
                    Log.Warn("[SCP-153] Brak sprawdzonego punktu HCZ; pozostawiam aktualna pozycje.");
                Log.Info("[SCP-153] " + result);
            });
        }
        private static int Life(Player player) => Lives.TryGetValue(player, out int generation) ? generation : 0;
        public static int CurrentLife(Player player) => Life(player);
        private static void OnSpawned(SpawnedEventArgs ev)
        {
            Lives[ev.Player] = Life(ev.Player) + 1;
            ForgetPlayerHits(ev.Player);
        }
        private static void OnLeft(LeftEventArgs ev)
        {
            ForgetPlayerHits(ev.Player);
            Lives.Remove(ev.Player);
            foreach (var p in Player.List)
                if (Scp153Component.TryGet(p, out var component)) component.Forget(ev.Player);
        }
        private static void ForgetPlayerHits(Player player)
        {
            foreach (var key in Pending.Where(p => p.Value.Victim == player || p.Value.Component.Player == player)
                .Select(p => p.Key).ToArray()) Pending.Remove(key);
        }
        public static void ForgetComponent(Scp153Component component)
        {
            foreach (var key in Pending.Where(p => p.Value.Component == component).Select(p => p.Key).ToArray())
                Pending.Remove(key);
        }
        public static void PrunePendingHits()
        {
            if (lastPrunedFrame == Time.frameCount) return;
            lastPrunedFrame = Time.frameCount;
            foreach (var key in Pending.Where(p => p.Value.Frame < Time.frameCount).Select(p => p.Key).ToArray())
                Pending.Remove(key); // Cancelled Hurting never emits Hurt.
        }

        private static void OnHurting(HurtingEventArgs ev)
        {
            if (!ev.IsAllowed || ev.Player == null || ev.Attacker == null ||
                ev.Attacker == ev.Player || !Scp153Component.TryGet(ev.Attacker, out var component)) return;
            var raw = ev.DamageHandler.Base as Scp049DamageHandler;
            if (raw == null || raw.DamageSubType != Scp049DamageHandler.AttackType.Scp0492) return;
            if (!component.AttackInProgress) return; // Not emitted by our native attack execution.
            if (component.BiteAwarded || !ev.Player.IsAlive || ev.Player.Role.Team == Team.SCPs || ev.Amount <= 0f)
            { ev.IsAllowed = false; return; }

            ev.Amount = component.Progress.NextDamage;
            Pending[raw] = new PendingHit(component, ev.Player, Life(ev.Player), ev.Player.Role.Base, round);
        }

        private static void OnHurt(HurtEventArgs ev)
        {
            var raw = ev.DamageHandler.Base;
            var standard = raw as StandardDamageHandler;
            float dealt = standard?.TotalDamageDealt ?? 0f;
            if (dealt > 0f && Scp153Component.TryGet(ev.Player, out var defender)) defender.RecordDamageTaken();
            if (!Pending.TryGetValue(raw, out var pending)) return;
            Pending.Remove(raw);

            var component = pending.Component;
            bool sameLife = pending.Round == round && pending.Victim == ev.Player &&
                Life(ev.Player) == pending.Life && ReferenceEquals(ev.Player.Role.Base, pending.Role);
            int previousTier = component.Progress.Tier;
            if (!component.CommitBite(pending.Attack, dealt, sameLife)) return;

            var progress = component.Progress;
            if (progress.Tier >= 4) component.Player.Heal(progress.Settings.FinalHealPerHit);
            if (progress.Tier != previousTier)
            {
                component.Player.Broadcast(7, $"<color=yellow><b>EWOLUCJA {progress.Tier}/4</b></color>\n" +
                    $"Ugryzienia: {progress.Count} | Maks. HS: {progress.MaxShield}");
                if (progress.Tier == 4 && progress.Settings.BlackoutDuration > 0f)
                {
                    Map.TurnOffAllLights(progress.Settings.BlackoutDuration);
                    foreach (var player in Player.List)
                        player.Broadcast(8, "<color=red><b>SCP-153 osiagnal forme ostateczna!</b></color>");
                }
            }
            else component.Player.ShowHint($"Ugryzienia: {progress.Count} | Maks. HS: {progress.MaxShield}", 4f);

            // All operations stay in the confirmed damage call. No stale victim callback survives.
            if (!SameLife(pending) || !ev.Player.IsAlive || ev.Player.Health <= 0f || Warhead.IsDetonated) return;
            if (progress.Tier >= 4 && progress.Settings.EnablePocketDimension)
            {
                if (ev.Player.IsInPocketDimension) return;
                ev.Player.EnableEffect(EffectType.PocketCorroding);
                if (SameLife(pending) && Room.Get(ev.Player.Position)?.Type == RoomType.Pocket)
                {
                    ev.Player.DropHeldItem();
                    ev.Player.ShowHint("<color=red>SCP-153 wciagnal cie do Wymiaru Lzowego!</color>", 6f);
                }
            }
            else if (TeleportService.TryRelocate(ev.Player))
                ev.Player.ShowHint("<color=red>SCP-153 przeniosl cie do innego korytarza.</color>", 4f);
        }

        private static bool SameLife(PendingHit hit) => hit.Victim?.Connection != null &&
            hit.Round == round && Life(hit.Victim) == hit.Life && ReferenceEquals(hit.Victim.Role.Base, hit.Role);

        private sealed class PendingHit
        {
            public readonly Scp153Component Component;
            public readonly Player Victim;
            public readonly int Life;
            public readonly PlayerRoleBase Role;
            public readonly int Round;
            public readonly int Frame;
            public readonly long Attack;
            public PendingHit(Scp153Component component, Player victim, int life, PlayerRoleBase role, int generation)
            {
                Component = component; Victim = victim; Life = life; Role = role; Round = generation;
                Attack = component.AttackId; Frame = Time.frameCount;
            }
        }
    }
}

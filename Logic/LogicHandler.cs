using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCP153.Logic
{
    public static class LogicHandler
    {
        private static readonly Dictionary<Player, float> EatCooldowns = new Dictionary<Player, float>();
        private static readonly Dictionary<Player, int> EatCounts = new Dictionary<Player, int>();

        private static readonly HashSet<RoomType> SafeCorridorRooms = new HashSet<RoomType>
        {
            RoomType.LczStraight,
            RoomType.LczCurve,
            RoomType.LczCrossing,
            RoomType.LczTCross,
            RoomType.LczCafe,
            RoomType.LczToilets,
            RoomType.HczStraight,
            RoomType.HczCurve,
            RoomType.HczCrossing,
            RoomType.HczArmory
        };

        public static void RegisterEvents()
        {
            Exiled.Events.Handlers.Player.Hurting += OnHurting;
            Exiled.Events.Handlers.Player.Left += OnLeft;
            Exiled.Events.Handlers.Player.ChangingSpectatedPlayer += OnChangingSpectatedPlayer;
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
        }

        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.Hurting -= OnHurting;
            Exiled.Events.Handlers.Player.Left -= OnLeft;
            Exiled.Events.Handlers.Player.ChangingSpectatedPlayer -= OnChangingSpectatedPlayer;
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
        }

        private static void OnChangingSpectatedPlayer(ChangingSpectatedPlayerEventArgs ev)
        {
            if (ev.Player == null) return;

            if (ev.OldTarget != null && ev.OldTarget.GameObject.TryGetComponent(out Scp153Component oldComp))
            {
                oldComp.ShowSchematicFor(ev.Player);
            }

            if (ev.NewTarget != null && ev.NewTarget.GameObject.TryGetComponent(out Scp153Component newComp))
            {
                Timing.CallDelayed(0.1f, () =>
                {
                    if (ev.Player != null && ev.NewTarget != null)
                        newComp.HideSchematicFor(ev.Player);
                });
            }
        }

        private static void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Player == null) return;

            foreach (Player p in Player.List)
            {
                if (p != ev.Player && p.GameObject.TryGetComponent(out Scp153Component comp))
                {
                    comp.ShowSchematicFor(ev.Player);
                }
            }
        }

        private static void OnLeft(LeftEventArgs ev)
        {
            if (ev.Player != null)
            {
                EatCooldowns.Remove(ev.Player);
                EatCounts.Remove(ev.Player);
            }
        }

        private static void OnRoundStarted()
        {
            EatCooldowns.Clear();
            EatCounts.Clear();

            Timing.CallDelayed(5.0f, () =>
            {
                if (Random.value > Plugin.Instance.Config.SpawnChance) return;
                if (!CustomRole.TryGet(153, out CustomRole bulsonRole)) return;

                var scpPool = Player.List.Where(p => p.Role.Team == Team.SCPs
                                                && p.Role.Type != RoleTypeId.Scp079
                                                && !CustomRole.Registered.Any(r => r.Check(p))).ToList();

                if (scpPool.Count == 0)
                {
                    Log.Warn("[SCP-153] Brak dostepnych SCP do przypisania roli (wszyscy maja juz CustomRole?)");
                    return;
                }

                Player selected = scpPool[Random.Range(0, scpPool.Count)];
                bulsonRole.AddRole(selected);
                EatCounts[selected] = 0;

                Timing.CallDelayed(1f, () =>
                {
                    if (selected == null || !selected.IsAlive) return;

                    var room = Room.List.FirstOrDefault(r => r.Type == RoomType.HczArmory)
                            ?? Room.List.FirstOrDefault(r => SafeCorridorRooms.Contains(r.Type) && r.Zone == ZoneType.HeavyContainment);

                    if (room != null)
                        selected.Teleport(GetSafeTeleportPosition(room));
                });

                Log.Info($"[SCP-153] Wylosowano gracza {selected.Nickname} z puli SCP.");
            });
        }

        private static void OnHurting(HurtingEventArgs ev)
        {
            if (ev.Player == null) return;

            if (!CustomRole.TryGet(153, out CustomRole bulsonRole)) return;

            if (bulsonRole.Check(ev.Player))
            {
                if (ev.Player.GameObject.TryGetComponent(out Scp153Component defenderComp))
                    defenderComp.RecordDamageTaken();

                if (ev.Attacker == null || ev.Attacker == ev.Player) return;
            }

            if (ev.Attacker == null || ev.Attacker == ev.Player) return;
            if (!bulsonRole.Check(ev.Attacker)) return;

            if (!EatCounts.ContainsKey(ev.Attacker))
                EatCounts[ev.Attacker] = 0;

            int currentEats = EatCounts[ev.Attacker];

            float currentCooldown = currentEats >= 7
                ? Mathf.Max(1f, Plugin.Instance.Config.EatCooldown - 1f)
                : Plugin.Instance.Config.EatCooldown;

            float now = Time.time;
            if (EatCooldowns.TryGetValue(ev.Attacker, out float last) && now - last < currentCooldown)
            {
                ev.IsAllowed = false;
                return;
            }

            EatCooldowns[ev.Attacker] = now;
            currentEats++;
            EatCounts[ev.Attacker] = currentEats;

            Scp153Component attackerComp = ev.Attacker.GameObject.GetComponent<Scp153Component>();
            if (attackerComp != null)
            {
                attackerComp.AddPermanentHumeShield(Plugin.Instance.Config.HumeShieldPerHit);
                attackerComp.PlayAttackSound();
            }

            ApplyEvolutionBuffs(ev.Attacker, attackerComp, currentEats);

            ev.Amount = currentEats >= 5 ? 75f : Plugin.Instance.Config.Damage;

            if (currentEats >= 10)
            {
                ev.Attacker.Heal(250f);
            }

            Timing.CallDelayed(0.1f, () =>
            {
                if (ev.Player == null || !ev.Player.IsAlive) return;

                if (currentEats >= 10)
                {
                    ev.Player.DropHeldItem();
                    ev.Player.EnableEffect(EffectType.PocketCorroding);
                    ev.Player.ShowHint("<color=red><b>SCP-153 wciągnął Cię do Wymiaru Łzowego i wytrącił Ci przedmiot z rąk!</b></color>", 8f);
                    return;
                }

                bool canUseLcz = !Map.IsLczDecontaminated;
                var safeRooms = Room.List.Where(r =>
                    SafeCorridorRooms.Contains(r.Type) &&
                    (r.Zone == ZoneType.HeavyContainment || (canUseLcz && r.Zone == ZoneType.LightContainment))
                ).ToList();

                if (safeRooms.Count > 0)
                {
                    Room targetRoom = safeRooms[Random.Range(0, safeRooms.Count)];
                    ev.Player.Teleport(GetSafeTeleportPosition(targetRoom));
                }
            });
        }

        private static Vector3 GetSafeTeleportPosition(Room room)
        {
            Vector3 origin = room.Position + Vector3.up * 2.0f;

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 5.0f))
            {
                return hit.point + Vector3.up * 1.15f;
            }

            return room.Position + Vector3.up * 1.25f;
        }

        private static void ApplyEvolutionBuffs(Player buli, Scp153Component comp, int eats)
        {
            float maxShield = comp != null ? comp.CurrentMaxHumeShield : Plugin.Instance.Config.BaseHumeShield;

            switch (eats)
            {
                case 3:
                    buli.DisableEffect(EffectType.Slowness);
                    buli.EnableEffect(EffectType.Slowness, 10, 9999f);
                    buli.ClearBroadcasts();
                    buli.Broadcast(8, $"<size=32><color=yellow><b>[EWOLUCJA 1/4]</b></color></size>\n<size=25>Pożarto <b>3/10</b> ofiar! Poruszasz się szybciej.\nMaksymalna tarcza: <color=#00FFFF><b>{maxShield}</b></color></size>");
                    break;

                case 5:
                    buli.ClearBroadcasts();
                    buli.Broadcast(8, $"<size=32><color=orange><b>[EWOLUCJA 2/4]</b></color></size>\n<size=25>Pożarto <b>5/10</b> ofiar! Twoje ugryzienia zadają teraz <b>75 DMG</b>.\nMaksymalna tarcza: <color=#00FFFF><b>{maxShield}</b></color></size>");
                    break;

                case 7:
                    buli.DisableEffect(EffectType.Slowness);
                    buli.EnableEffect(EffectType.MovementBoost, 10, 9999f);
                    buli.ClearBroadcasts();
                    buli.Broadcast(8, $"<size=32><color=#FF4500><b>[EWOLUCJA 3/4]</b></color></size>\n<size=25>Pożarto <b>7/10</b> ofiar! Odzyskujesz pełną prędkość i gryziesz szybciej!\nMaksymalna tarcza: <color=#00FFFF><b>{maxShield}</b></color></size>");
                    break;

                case 10:
                    buli.EnableEffect(EffectType.MovementBoost, 20, 9999f);
                    if (comp != null) comp.RegenMultiplier = 2f;

                    Map.TurnOffAllLights(15f);

                    // Potężny Broadcast specjalnie dla gracza grającego Bulim
                    buli.ClearBroadcasts();
                    buli.Broadcast(12, $"<size=45><color=red><b>BULI SIĘ NAŻARŁ!</b></color></size>\n<size=26><color=yellow>Osiągnąłeś formę ostateczną! Rozbrajasz ofiary, wysyłasz je do <b>Wymiaru Łzowego</b> i leczysz się przy ataku!</color>\nMaksymalna tarcza: <color=#00FFFF><b>{maxShield}</b></color></size>");

                    // Ostrzeżenie dla wszystkich pozostałych graczy na serwerze
                    foreach (Player p in Player.List)
                    {
                        if (p != buli)
                        {
                            p.Broadcast(12, "<b><size=35><color=red>[KRYTYCZNE ZAGROŻENIE]</color></size></b>\n<size=28><color=yellow>SCP-153</color> osiągnął formę ostateczną! Systemy zasilania w placówce uległy przeciążeniu!</size>");
                        }
                    }
                    break;

                default:
                    // Pokazujemy licznik TYLKO przed wbiciem 10 ugryzień (powyżej 10 nic się już nie wyświetla)
                    if (eats < 10)
                    {
                        buli.ShowHint($"Pożarte ofiary: <color=red><b>{eats}/10</b></color> | Maks. Tarcza: <color=#00FFFF><b>{maxShield}</b></color>", 6f);
                    }
                    break;
            }
        }
    }
}
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using Mirror;
using HarmonyLib;
using PlayerRoles;
using PlayerRoles.PlayableScps.HumeShield;
using PlayerRoles.PlayableScps.Scp049.Zombies;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using SCP153.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SCP153
{
    public sealed class Scp153Component : MonoBehaviour
    {
        private const string ClipName = "scp153.haps.v2";
        private static readonly MethodInfo AddObserver = AccessTools.Method(typeof(NetworkIdentity), "AddObserver");
        private static readonly MethodInfo RemoveObserver = AccessTools.Method(typeof(NetworkIdentity), "RemoveObserver");
        private static readonly MethodInfo RemoveFromObserving = AccessTools.Method(typeof(NetworkConnectionToClient), "RemoveFromObserving");
        private Player player;
        private Plugin plugin;
        private PlayerRoleBase baseRole;
        private SchematicObject schematic;
        private NetworkIdentity[] identities = Array.Empty<NetworkIdentity>();
        private AudioPlayer audio;
        private Speaker speaker;
        private bool stopped;
        private float lastDamageTime;
        private float smoothY;
        private float nextVisibilitySync;
        private PlayerInfoArea previousInfoArea;
        private string previousCustomInfo;
        private string previousUniqueRole;
        private bool ownsInfoArea;
        private readonly HashSet<Player> hidden = new HashSet<Player>();
        private readonly Dictionary<EffectType, EffectLease> effects = new Dictionary<EffectType, EffectLease>();

        public BiteProgress Progress { get; private set; }
        public Player Player => player;
        public bool AttackInProgress { get; private set; }
        public bool BiteAwarded { get; private set; }
        public long AttackId { get; private set; }
        public bool IsActive => !stopped && plugin != null && plugin.IsRunning &&
            player?.Connection != null && player.IsAlive && ReferenceEquals(player.Role.Base, baseRole) &&
            plugin.Role != null && plugin.Role.Check(player);

        public static bool TryGet(Player target, out Scp153Component component)
        {
            component = null;
            if (target?.GameObject == null) return false;
            foreach (var candidate in target.GameObject.GetComponents<Scp153Component>())
                if (candidate != null && candidate.IsActive) { component = candidate; return true; }
            return false;
        }

        public void Initialize(Player owner, Plugin ownerPlugin)
        {
            player = owner;
            plugin = ownerPlugin;
            baseRole = owner.Role.Base;
            Progress = new BiteProgress(ownerPlugin.Config);
            previousCustomInfo = player.CustomInfo;
            previousUniqueRole = player.UniqueRole;
            player.CustomInfo = ownerPlugin.Role.CustomInfo;
            player.UniqueRole = ownerPlugin.Role.Name;
            lastDamageTime = Time.time;
            player.MaxHumeShield = Progress.MaxShield;
            player.HumeShield = Progress.MaxShield;
            ApplyMovement();

            try
            {
                if (AddObserver == null || RemoveObserver == null || RemoveFromObserving == null)
                    throw new InvalidOperationException("Nieobslugiwana wersja API widocznosci Mirror.");
                schematic = ObjectSpawner.SpawnSchematic(ownerPlugin.Config.SchematicName, player.Position);
                if (schematic == null) throw new InvalidOperationException("SpawnSchematic zwrocil null.");
                identities = schematic.NetworkIdentities.Where(i => i != null).ToArray();
                if (identities.Length == 0) throw new InvalidOperationException("Schemat nie ma obiektow sieciowych.");
                schematic.transform.parent = null;
                smoothY = player.Position.y - 0.95f;
                schematic.transform.position = player.Position + Vector3.down * 0.95f;
                previousInfoArea = player.InfoArea;
                ownsInfoArea = true;
                player.InfoArea = (PlayerInfoArea)0;
                SetOwnedEffect(EffectType.Fade, 255);
                SynchronizeVisibility();
            }
            catch (Exception ex)
            {
                Log.Error("[SCP-153] Model niedostepny; pozostawiam widoczne cialo Zombie: " + ex.Message);
                DestroyModel();
                RestoreEffect(EffectType.Fade);
                RestoreInfoArea();
            }
            InitializeAudio();
        }

        public void BeginAttack()
        {
            if (!IsActive) return;
            AttackId++;
            AttackInProgress = true;
            BiteAwarded = false;
        }
        public void EndAttack() => AttackInProgress = false;
        public void RecordDamageTaken() => lastDamageTime = Time.time;

        public bool CommitBite(long attack, float damage, bool sameLife)
        {
            if (!IsActive || !AttackInProgress || attack != AttackId || BiteAwarded ||
                !Progress.TryCommit(attack, damage, sameLife)) return false;
            BiteAwarded = true;
            player.MaxHumeShield = Progress.MaxShield;
            player.HumeShield = Progress.ShieldAfterBite(player.HumeShield);
            ApplyMovement();
            if (audio != null)
            {
                try { audio.AddClip(ClipName); }
                catch (Exception ex)
                {
                    Log.Warn("[SCP-153] Wylaczam niedostepne audio: " + ex.Message);
                    DestroyAudio();
                }
            }
            return true;
        }

        private void ApplyMovement()
        {
            SetOwnedEffect(EffectType.Slowness, Progress.Slowness);
            SetOwnedEffect(EffectType.MovementBoost, Progress.MovementBoost);
        }

        private void InitializeAudio()
        {
            try
            {
                if (!AudioClipStorage.AudioClips.ContainsKey(ClipName))
                {
                    string path = Path.Combine(Paths.Configs, "Plugins", "scp153", "Audio", "HAPS.ogg");
                    if (!File.Exists(path) || !AudioClipStorage.LoadClip(path, ClipName))
                    { Log.Warn("[SCP-153] Brak poprawnego audio: " + path); return; }
                }
                audio = AudioPlayer.CreateOrGet("SCP153-" + player.Id + "-" + Guid.NewGuid().ToString("N"));
                speaker = audio.AddSpeaker("Main", isSpatial: true, minDistance: 1f, maxDistance: 20f);
            }
            catch (Exception ex)
            {
                Log.Warn("[SCP-153] Audio niedostepne: " + ex.Message);
                DestroyAudio();
            }
        }

        private void Update()
        {
            if (stopped || Progress == null) return;
            if (!IsActive) { Shutdown(); Destroy(this); return; }
            LogicHandler.PrunePendingHits();
            bool blocked = (player.Role as Scp0492Role)?.HumeShieldModule is DynamicHumeShieldController shield && shield.IsBlocked;
            if (player.MaxHumeShield != Progress.MaxShield) player.MaxHumeShield = Progress.MaxShield;
            float shieldNow = player.HumeShield;
            float shieldNext = Progress.Regenerate(shieldNow, Time.time - lastDamageTime, Time.deltaTime, blocked);
            if (shieldNext != shieldNow) player.HumeShield = shieldNext;

            Vector3 position = player.Position;
            if (schematic != null)
            {
                float targetY = position.y - 0.95f;
                smoothY = Mathf.Abs(smoothY - targetY) > 4f ? targetY : Mathf.Lerp(smoothY, targetY, Time.deltaTime * 8f);
                schematic.transform.position = new Vector3(position.x, smoothY, position.z);
                schematic.transform.rotation = Quaternion.Euler(0f, player.Rotation.eulerAngles.y, 0f);
            }
            else if (ownsInfoArea)
            {
                // ProjectMER may remove a model while the role is alive.
                RestoreEffect(EffectType.Fade);
                RestoreInfoArea();
                identities = Array.Empty<NetworkIdentity>();
                hidden.Clear();
            }
            if (audio != null && speaker != null)
            {
                audio.transform.position = position;
                speaker.transform.position = position;
            }
            if (Time.time >= nextVisibilitySync)
            {
                nextVisibilitySync = Time.time + 0.1f;
                SynchronizeVisibility();
            }
        }

        public void SynchronizeVisibility()
        {
            if (stopped || schematic == null) return;
            try
            {
            foreach (var target in Player.List)
            {
                var connection = target.Connection as NetworkConnectionToClient;
                if (connection == null || !connection.isReady) continue;
                bool shouldHide = target == player ||
                    (target.Role is Exiled.API.Features.Roles.SpectatorRole spectator && spectator.SpectatedPlayer == player);
                if (shouldHide)
                {
                    hidden.Add(target);
                    foreach (var identity in identities)
                    {
                        if (identity == null || !identity.observers.ContainsKey(connection.connectionId)) continue;
                        // Keep both sides of Mirror's observer relationship consistent.
                        RemoveObserver.Invoke(identity, new object[] { connection });
                        RemoveFromObserving.Invoke(connection, new object[] { identity, false });
                    }
                }
                else if (hidden.Remove(target))
                {
                    foreach (var identity in identities)
                        if (identity != null) AddObserver.Invoke(identity, new object[] { connection });
                }
            }
            }
            catch (Exception ex)
            {
                Log.Error("[SCP-153] Blad widocznosci; przywracam widoczne cialo Zombie: " + ex.Message);
                DestroyModel();
                RestoreEffect(EffectType.Fade);
                RestoreInfoArea();
                hidden.Clear();
            }
        }

        public void Forget(Player target) => hidden.Remove(target);

        private void SetOwnedEffect(EffectType type, byte intensity)
        {
            if (intensity == 0) { RestoreEffect(type); return; }
            var effect = player.GetEffect(type);
            if (effect == null) return;
            if (!effects.TryGetValue(type, out var lease))
            {
                lease = new EffectLease(effect.Intensity, effect.TimeLeft);
                effects.Add(type, lease);
            }
            else if (effect.Intensity != lease.Applied || effect.TimeLeft > 0f)
                return; // Another system changed the effect: do not overwrite it.
            player.EnableEffect(type, intensity, 0f);
            lease.Applied = intensity;
        }

        private void RestoreEffect(EffectType type)
        {
            if (!effects.TryGetValue(type, out var lease)) return;
            effects.Remove(type);
            if (player?.Connection == null) return;
            var effect = player.GetEffect(type);
            if (effect == null || effect.Intensity != lease.Applied || effect.TimeLeft > 0f) return;
            float remaining = lease.WasTimed ? Mathf.Max(0f, lease.Expiry - Time.time) : 0f;
            if (lease.Original == 0 || (lease.WasTimed && remaining <= 0f)) player.DisableEffect(type);
            else player.EnableEffect(type, lease.Original, remaining);
        }

        private void RestoreInfoArea()
        {
            if (ownsInfoArea && player?.Connection != null && player.InfoArea == (PlayerInfoArea)0)
                player.InfoArea = previousInfoArea;
            ownsInfoArea = false;
        }

        public void Shutdown()
        {
            if (stopped) return;
            stopped = true;
            AttackInProgress = false;
            LogicHandler.ForgetComponent(this);
            if (player?.Connection != null)
            {
                RestoreInfoArea();
                if (player.CustomInfo == plugin?.Role?.CustomInfo) player.CustomInfo = previousCustomInfo;
                if (player.UniqueRole == plugin?.Role?.Name) player.UniqueRole = previousUniqueRole;
            }
            // Restore only while still in the life that owned these changes.
            if (player?.Connection != null && ReferenceEquals(player.Role.Base, baseRole))
            {
                foreach (var type in effects.Keys.ToArray()) RestoreEffect(type);
                RestoreInfoArea();
                if (player.Role is Scp0492Role zombie)
                {
                    if (player.MaxHealth == Progress?.Settings.MaxHealth)
                    {
                        player.MaxHealth = ((ZombieRole)baseRole).MaxHealth;
                        player.Health = Mathf.Min(player.Health, player.MaxHealth);
                    }
                    if (Progress != null && player.MaxHumeShield == Progress.MaxShield)
                    {
                        player.HumeShield = 0f;
                        player.MaxHumeShield = float.MinValue; // Return max ownership to the native controller.
                    }
                }
            }
            effects.Clear();
            hidden.Clear();
            DestroyModel();
            DestroyAudio();
        }

        private void DestroyModel()
        {
            var old = schematic;
            schematic = null;
            identities = Array.Empty<NetworkIdentity>();
            if (old != null)
                try { old.Destroy(); }
                catch (Exception ex) { Log.Error("[SCP-153] Blad usuwania modelu: " + ex.Message); }
        }
        private void DestroyAudio()
        {
            var old = audio;
            audio = null;
            speaker = null;
            if (old != null)
                try { old.Destroy(); }
                catch (Exception ex) { Log.Error("[SCP-153] Blad usuwania audio: " + ex.Message); }
        }
        private void OnDestroy() => Shutdown();

        private sealed class EffectLease
        {
            public readonly byte Original;
            public readonly bool WasTimed;
            public readonly float Expiry;
            public byte Applied;
            public EffectLease(byte original, float remaining)
            {
                Original = original;
                WasTimed = remaining > 0f;
                Expiry = Time.time + remaining;
            }
        }
    }
}

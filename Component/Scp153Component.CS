using Exiled.API.Features;
using MEC;
using Mirror;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace SCP153
{
    public class Scp153Component : MonoBehaviour
    {
        private Player _player;
        private SchematicObject _schematic;

        private AudioPlayer _audioPlayer;
        private Speaker _speaker;

        private readonly HashSet<Player> _hiddenForSpectators = new HashSet<Player>();

        private static readonly MethodInfo SendSpawnMessageMethod = typeof(NetworkServer).GetMethod(
            "SendSpawnMessage",
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static
        );

        private string AudioPath => Path.Combine(Paths.Configs, "Plugins", "scp153", "Audio");

        private float _smoothY;
        private const float YSmoothSpeed = 8f;

        private const float OffsetX = 0f;
        private const float OffsetY = -0.95f;
        private const float OffsetZ = 0f;

        public float CurrentMaxHumeShield { get; private set; }
        public float RegenMultiplier { get; set; } = 1f;
        private float _lastDamageTime;

        private void Start()
        {
            _player = Player.Get(gameObject);
            if (_player == null || Plugin.Instance == null)
            {
                Destroy(this);
                return;
            }

            CurrentMaxHumeShield = Plugin.Instance.Config.BaseHumeShield;
            string schemName = Plugin.Instance.Config.SchematicName;

            try
            {
                _schematic = ObjectSpawner.SpawnSchematic(schemName, _player.Position);

                if (_schematic != null)
                {
                    _schematic.transform.parent = null;
                    _schematic.transform.localScale = Vector3.one;
                    _smoothY = _player.Position.y + OffsetY;

                    Timing.CallDelayed(0.5f, () => HideSchematicFor(_player));
                }
            }
            catch (Exception e)
            {
                Log.Error($"[SCP-153] BŁĄD schematu: {e}");
            }

            Timing.CallDelayed(1.2f, () =>
            {
                if (_player == null || !_player.IsAlive) return;

                try
                {
                    string filePath = Path.Combine(AudioPath, "HAPS.ogg");
                    if (!File.Exists(filePath))
                    {
                        Log.Warn($"[SCP-153] Brak pliku audio: {filePath}");
                        return;
                    }

                    if (!AudioClipStorage.AudioClips.ContainsKey("HAPS"))
                    {
                        AudioClipStorage.LoadClip(filePath, "HAPS");
                    }

                    string botId = $"SCP153-{_player.Id}-{UnityEngine.Random.Range(1, 9999)}";

                    _audioPlayer = AudioPlayer.CreateOrGet(botId, onIntialCreation: (p) =>
                    {
                        _speaker = p.AddSpeaker("Main", isSpatial: true, minDistance: 1f, maxDistance: 20f);
                    });
                }
                catch (Exception e)
                {
                    Log.Error($"[SCP-153] Audio Init Error: {e}");
                }
            });
        }

        public void HideSchematicFor(Player target)
        {
            if (target?.Connection == null || _schematic == null) return;

            if (target != _player)
                _hiddenForSpectators.Add(target);

            foreach (NetworkIdentity identity in _schematic.NetworkIdentities)
            {
                if (identity != null)
                {
                    target.Connection.Send(new ObjectDestroyMessage { netId = identity.netId });
                }
            }
        }

        public void ShowSchematicFor(Player target)
        {
            if (target == null || !_hiddenForSpectators.Remove(target)) return;
            if (target.Connection == null || _schematic == null || SendSpawnMessageMethod == null) return;

            foreach (NetworkIdentity identity in _schematic.NetworkIdentities)
            {
                if (identity != null)
                {
                    SendSpawnMessageMethod.Invoke(null, new object[] { identity, target.Connection });
                }
            }
        }

        public void AddPermanentHumeShield(float amount)
        {
            float limit = Plugin.Instance.Config.MaxHumeShieldLimit;
            CurrentMaxHumeShield = Mathf.Min(limit, CurrentMaxHumeShield + amount);

            if (_player != null && _player.IsAlive)
            {
                _player.MaxHumeShield = CurrentMaxHumeShield;
                _player.HumeShield = Mathf.Min(CurrentMaxHumeShield, _player.HumeShield + amount);
            }
        }

        public void RecordDamageTaken()
        {
            _lastDamageTime = Time.time;
        }

        public void PlayAttackSound()
        {
            if (_audioPlayer != null)
                _audioPlayer.AddClip("HAPS");
        }

        private void Update()
        {
            if (_player == null || !_player.IsAlive) return;

            if (_player.MaxHumeShield != CurrentMaxHumeShield)
                _player.MaxHumeShield = CurrentMaxHumeShield;

            if (_player.HumeShield < CurrentMaxHumeShield && Time.time - _lastDamageTime >= Plugin.Instance.Config.HumeShieldRegenDelay)
            {
                float regen = Plugin.Instance.Config.HumeShieldRegenRate * RegenMultiplier * Time.deltaTime;
                _player.HumeShield = Mathf.Min(CurrentMaxHumeShield, _player.HumeShield + regen);
            }

            Vector3 playerPos = _player.Position;
            _smoothY = Mathf.Lerp(_smoothY, playerPos.y + OffsetY, Time.deltaTime * YSmoothSpeed);

            float yaw = _player.Rotation.eulerAngles.y;
            Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);

            Vector3 localOffset = new Vector3(OffsetX, 0f, OffsetZ);
            Vector3 worldOffset = yawRotation * localOffset;

            if (_schematic != null)
            {
                _schematic.transform.position = new Vector3(
                    playerPos.x + worldOffset.x,
                    _smoothY,
                    playerPos.z + worldOffset.z
                );
                _schematic.transform.rotation = yawRotation;
            }

            if (_audioPlayer != null && _speaker != null)
            {
                _audioPlayer.transform.position = playerPos;
                _speaker.transform.position = playerPos;
            }
        }

        private void OnDestroy()
        {
            _hiddenForSpectators.Clear();
            if (_schematic != null) _schematic.Destroy();
            if (_audioPlayer != null) _audioPlayer.Destroy();
        }
    }
}
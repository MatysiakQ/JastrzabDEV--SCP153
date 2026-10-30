using Exiled.API.Features;
using Exiled.CustomRoles.API;
using Exiled.CustomRoles.API.Features;
using HarmonyLib;
using SCP153.Logic;
using System;
using System.Linq;

namespace SCP153
{
    public sealed class Plugin : Plugin<Config>
    {
        public static Plugin Instance { get; private set; }
        public Roles.Scp153 Role { get; private set; }
        public bool IsRunning { get; private set; }
        private Harmony harmony;
        public override string Name => "SCP153";
        public override string Prefix => "scp153";
        public override string Author => "Matysiak";
        public override Version Version => new Version(2, 2, 0);
        public override Version RequiredExiledVersion => new Version(9, 12, 6);

        public override void OnEnabled()
        {
            var errors = Config.Validate();
            if (errors.Count != 0)
            {
                Log.Error("[SCP-153] Nieprawidlowa konfiguracja: " + string.Join(" ", errors));
                return;
            }
            if (CustomRole.Registered.Any(r => r.Id == Config.CustomRoleId))
            {
                Log.Error($"[SCP-153] CustomRole ID {Config.CustomRoleId} jest juz zajete. Plugin nie zostal uruchomiony.");
                return;
            }
            Instance = this;
            Role = new Roles.Scp153(this);
            try
            {
                Role.Register();
                if (!CustomRole.Registered.Contains(Role))
                    throw new InvalidOperationException("CustomRoles nie zarejestrowalo SCP-153 (sprawdz modul i config).");
                harmony = new Harmony("Matysiak.SCP153.2.2.0");
                harmony.PatchAll();
                LogicHandler.RegisterEvents();
                IsRunning = true;
                base.OnEnabled();
                Log.Info("[SCP-153] Wersja 2.2.0 uruchomiona.");
            }
            catch (Exception ex)
            {
                Log.Error("[SCP-153] Blad uruchamiania: " + ex);
                Stop();
            }
        }
        public override void OnDisabled()
        {
            Stop();
            base.OnDisabled();
        }
        private void Stop()
        {
            if (Instance != this && Role == null) return;
            IsRunning = false;
            try
            {
                LogicHandler.UnregisterEvents();
                Role?.DisposePlayers();
                if (Role != null && CustomRole.Registered.Contains(Role)) Role.Unregister();
            }
            finally
            {
                harmony?.UnpatchAll(harmony.Id);
                harmony = null;
                Role = null;
                if (ReferenceEquals(Instance, this)) Instance = null;
            }
        }
    }
}

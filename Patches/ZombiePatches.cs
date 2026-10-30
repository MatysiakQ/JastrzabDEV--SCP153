using Exiled.API.Features;
using Exiled.API.Features.Roles;
using HarmonyLib;
using PlayerRoles.PlayableScps.HumeShield;
using PlayerRoles.PlayableScps.Scp049.Zombies;
using PlayerRoles.PlayableScps.Subroutines;
using System;

namespace SCP153.Patches
{
    [HarmonyPatch(typeof(ZombieAttackAbility), "get_BaseCooldown")]
    internal static class AttackCooldownPatch
    {
        private static void Postfix(ZombieAttackAbility __instance, ref float __result)
        {
            if (Scp153Component.TryGet(Player.Get(__instance.Owner), out var component))
                __result = component.Progress.Cooldown;
        }
    }

    [HarmonyPatch(typeof(ScpAttackAbilityBase<ZombieRole>), "ServerPerformAttack")]
    internal static class AttackScopePatch
    {
        private static void Prefix(ScpAttackAbilityBase<ZombieRole> __instance)
        {
            if (__instance is ZombieAttackAbility && Scp153Component.TryGet(Player.Get(__instance.Owner), out var component))
                component.BeginAttack();
        }
        private static Exception Finalizer(ScpAttackAbilityBase<ZombieRole> __instance, Exception __exception)
        {
            if (__instance is ZombieAttackAbility && Scp153Component.TryGet(Player.Get(__instance.Owner), out var component))
                component.EndAttack();
            return __exception;
        }
    }

    [HarmonyPatch(typeof(ZombieShieldController), "get_HsRegeneration")]
    internal static class NativeShieldRegenPatch
    {
        private static bool Prefix(ZombieShieldController __instance, ref float __result)
        {
            var plugin = Plugin.Instance;
            if (plugin?.IsRunning != true || plugin.Role == null) return true;
            foreach (var player in plugin.Role.TrackedPlayers)
                if (player.Role is Scp0492Role role && ReferenceEquals(role.HumeShieldModule, __instance) &&
                    Scp153Component.TryGet(player, out _))
                {
                    __result = 0f; // Keep normal stat synchronization; suppress only our Zombie's regen.
                    return false;
                }
            return true;
        }
    }
}

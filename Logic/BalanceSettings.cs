using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SCP153.Logic
{
    // Shared by the production plugin and server-independent regression tests.
    public class BalanceSettings
    {
        public const float NativeClientAttackInterval = 1.3f;
        public float SpawnChance { get; set; } = 0.1f;
        public int MaxHealth { get; set; } = 4500;
        [Description("Poczatkowa, regenerujaca sie tarcza HS.")]
        public float BaseHumeShield { get; set; } = 400f;
        public float MaxHumeShieldLimit { get; set; } = 2000f;
        [Description("Bonus do maksymalnego i aktualnego HS za skuteczne ugryzienie.")]
        public float HumeShieldPerHit { get; set; } = 50f;
        public float HumeShieldRegenRate { get; set; } = 15f;
        public float HumeShieldRegenDelay { get; set; } = 5f;
        public byte SlownessIntensity { get; set; } = 20;
        public float Damage { get; set; } = 50f;
        [Description("Cooldown serwerowy ataku; minimum 1.3 s (natywny klient Zombie).")]
        public float EatCooldown { get; set; } = 3f;
        public int EvolutionOneHits { get; set; } = 3;
        public int EvolutionTwoHits { get; set; } = 5;
        public int EvolutionThreeHits { get; set; } = 7;
        public int EvolutionFourHits { get; set; } = 10;
        public float EvolvedDamageMultiplier { get; set; } = 1.5f;
        public float FirstEvolutionSlownessMultiplier { get; set; } = 0.5f;
        public byte ThirdEvolutionMovementBoost { get; set; } = 10;
        public byte FinalEvolutionMovementBoost { get; set; } = 20;
        public float CooldownReduction { get; set; } = 1f;
        public float FinalHealPerHit { get; set; } = 250f;
        public float FinalRegenMultiplier { get; set; } = 2f;
        public float BlackoutDuration { get; set; } = 15f;
        public bool EnablePocketDimension { get; set; } = true;
        [Description("Usuniecie roli przenosi gracza do Spectator.")]
        public bool RemovalKillsPlayer { get; set; } = true;

        public IReadOnlyList<string> ValidateBalance()
        {
            var errors = new List<string>();
            Range(errors, "spawn_chance", SpawnChance, 0f, 1f);
            if (MaxHealth <= 0) errors.Add("max_health musi byc wieksze od 0.");
            Range(errors, "max_hume_shield_limit", MaxHumeShieldLimit, 0f, 65535f);
            Range(errors, "base_hume_shield", BaseHumeShield, 0f, MaxHumeShieldLimit);
            Range(errors, "hume_shield_per_hit", HumeShieldPerHit, 0f, 65535f);
            NonNegative(errors, "hume_shield_regen_rate", HumeShieldRegenRate);
            NonNegative(errors, "hume_shield_regen_delay", HumeShieldRegenDelay);
            if (!Finite(Damage) || Damage <= 0f) errors.Add("damage musi byc dodatnie i skonczone.");
            if (!Finite(EatCooldown) || EatCooldown < NativeClientAttackInterval)
                errors.Add("eat_cooldown musi byc skonczone i >= 1.3 s.");
            if (!(0 < EvolutionOneHits && EvolutionOneHits < EvolutionTwoHits &&
                  EvolutionTwoHits < EvolutionThreeHits && EvolutionThreeHits < EvolutionFourHits))
                errors.Add("Progi evolution_*_hits musza byc dodatnie i scisle rosnace.");
            if (!Finite(EvolvedDamageMultiplier) || EvolvedDamageMultiplier < 1f ||
                !Finite(Damage * EvolvedDamageMultiplier))
                errors.Add("evolved_damage_multiplier musi byc >= 1 i dawac skonczone obrazenia.");
            Range(errors, "first_evolution_slowness_multiplier", FirstEvolutionSlownessMultiplier, 0f, 1f);
            if (FinalEvolutionMovementBoost < ThirdEvolutionMovementBoost)
                errors.Add("final_evolution_movement_boost nie moze byc mniejsze od third_evolution_movement_boost.");
            NonNegative(errors, "cooldown_reduction", CooldownReduction);
            NonNegative(errors, "final_heal_per_hit", FinalHealPerHit);
            if (!Finite(FinalRegenMultiplier) || FinalRegenMultiplier < 1f)
                errors.Add("final_regen_multiplier musi byc skonczone i >= 1.");
            if (!Finite(HumeShieldRegenRate * FinalRegenMultiplier))
                errors.Add("Iloczyn regeneracji i final_regen_multiplier musi byc skonczony.");
            NonNegative(errors, "blackout_duration", BlackoutDuration);
            return errors;
        }

        internal BalanceSettings Snapshot() => (BalanceSettings)MemberwiseClone();
        internal static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static void NonNegative(List<string> errors, string key, float value)
        {
            if (!Finite(value) || value < 0f) errors.Add(key + " musi byc nieujemne i skonczone.");
        }
        private static void Range(List<string> errors, string key, float value, float min, float max)
        {
            if (!Finite(value) || !Finite(max) || value < min || value > max)
                errors.Add(key + " musi nalezec do [" + min + ", " + max + "].");
        }
    }
}

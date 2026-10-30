using System;

namespace SCP153.Logic
{
    public sealed class BiteProgress
    {
        private long lastCommittedAttack;
        public BalanceSettings Settings { get; }
        public int Count { get; private set; }
        public int Tier => Count >= Settings.EvolutionFourHits ? 4 :
                           Count >= Settings.EvolutionThreeHits ? 3 :
                           Count >= Settings.EvolutionTwoHits ? 2 :
                           Count >= Settings.EvolutionOneHits ? 1 : 0;
        public float MaxShield => (float)Math.Min(Settings.MaxHumeShieldLimit,
            Settings.BaseHumeShield + (double)Count * Settings.HumeShieldPerHit);
        public float NextDamage => Settings.Damage *
            (Count >= Settings.EvolutionTwoHits - 1 ? Settings.EvolvedDamageMultiplier : 1f);
        public float Cooldown => Tier >= 3
            ? Math.Max(BalanceSettings.NativeClientAttackInterval, Settings.EatCooldown - Settings.CooldownReduction)
            : Settings.EatCooldown;
        public byte Slowness => Tier >= 3 ? (byte)0 : Tier >= 1
            ? (byte)(Settings.SlownessIntensity * Settings.FirstEvolutionSlownessMultiplier)
            : Settings.SlownessIntensity;
        public byte MovementBoost => Tier >= 4 ? Settings.FinalEvolutionMovementBoost :
                                     Tier >= 3 ? Settings.ThirdEvolutionMovementBoost : (byte)0;

        public BiteProgress(BalanceSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            var errors = settings.ValidateBalance();
            if (errors.Count != 0) throw new ArgumentException(string.Join(" ", errors), nameof(settings));
            Settings = settings.Snapshot();
        }

        // Commit at most once per native swing, only after actual damage to the same life.
        public bool TryCommit(long attack, float damageDealt, bool sameLife)
        {
            if (!sameLife || attack <= lastCommittedAttack || !BalanceSettings.Finite(damageDealt) || damageDealt <= 0f)
                return false;
            lastCommittedAttack = attack;
            if (Count < int.MaxValue) Count++;
            return true;
        }
        public float ShieldAfterBite(float current) => Math.Min(MaxShield,
            Math.Max(0f, current) + Settings.HumeShieldPerHit);
        public float Regenerate(float current, float secondsSinceDamage, float deltaTime, bool blocked)
        {
            current = Math.Max(0f, Math.Min(MaxShield, current));
            if (blocked || secondsSinceDamage < Settings.HumeShieldRegenDelay || deltaTime <= 0f) return current;
            double rate = Settings.HumeShieldRegenRate * (Tier >= 4 ? Settings.FinalRegenMultiplier : 1f);
            return (float)Math.Min(MaxShield, current + rate * deltaTime);
        }
    }
}

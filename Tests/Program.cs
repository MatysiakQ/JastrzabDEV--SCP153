using System;
using SCP153.Logic;

internal static class Program
{
    private static int passed;
    private static void Main()
    {
        Test("Fresh role starts at 400 HS", () => Equal(400, New().MaxShield));
        Test("Every confirmed bite adds 50 maximum HS", () => { var p = Grow(1); Equal(450, p.MaxShield); });
        Test("Final evolution has 900 HS", () => Equal(900, Grow(10).MaxShield));
        Test("32 bites reach the 2000 limit", () => Equal(2000, Grow(32).MaxShield));
        Test("Further bites never exceed the limit", () => Equal(2000, Grow(100).MaxShield));
        Test("Bite restores 50 current HS, not the whole shield", () => Equal(200, Grow(1).ShieldAfterBite(150)));
        Test("At the cap each bite can still restore 50 HS", () => Equal(1900, Grow(40).ShieldAfterBite(1850)));
        Test("Regeneration waits for five seconds without damage", () => Equal(200, New().Regenerate(200, 4.99f, 1, false)));
        Test("Base regeneration is 15 HS per second", () => Equal(215, New().Regenerate(200, 5, 1, false)));
        Test("Final regeneration is 30 HS per second", () => Equal(230, Grow(10).Regenerate(200, 5, 1, false)));
        Test("Regeneration respects native blocking", () => Equal(200, Grow(10).Regenerate(200, 20, 1, true)));
        Test("Regeneration stops at the earned cap", () => Equal(450, Grow(1).Regenerate(440, 20, 1, false)));
        Test("Zero damage grants no progress", () => { var p = New(); Check(!p.TryCommit(1, 0, true)); Equal(0, p.Count); });
        Test("Invalid numeric damage grants no progress", () =>
        {
            var p = New();
            Check(!p.TryCommit(1, float.NaN, true) && !p.TryCommit(1, float.PositiveInfinity, true));
            Equal(0, p.Count);
        });
        Test("A different life cannot receive a pending bite", () => Check(!New().TryCommit(1, 50, false)));
        Test("One native swing grants at most one bite", () =>
        {
            var p = New(); Check(p.TryCommit(1, 50, true)); Check(!p.TryCommit(1, 50, true)); Equal(1, p.Count);
        });
        Test("Old attack receipts cannot be replayed", () =>
        {
            var p = New(); Check(p.TryCommit(5, 50, true)); Check(!p.TryCommit(4, 50, true)); Equal(1, p.Count);
        });
        Test("Rejected receipt does not consume a valid attack", () =>
        {
            var p = New(); Check(!p.TryCommit(1, 0, true)); Check(p.TryCommit(1, 50, true));
        });
        Test("New role never inherits old evolution", () => { var old = Grow(12); var fresh = New(); Equal(0, fresh.Tier); Equal(400, fresh.MaxShield); });
        Test("Fifth bite applies evolved damage", () => { Equal(50, Grow(3).NextDamage); Equal(75, Grow(4).NextDamage); });
        Test("Configured damage cannot decrease at evolution", () =>
        {
            var p = Grow(5, new BalanceSettings { Damage = 100 }); Equal(150, p.NextDamage);
        });
        Test("Disabled slowness remains disabled after evolution", () =>
        {
            var p = Grow(3, new BalanceSettings { SlownessIntensity = 0 }); Equal(0, p.Slowness);
        });
        Test("Weak configured slowness never increases", () =>
        {
            var p = Grow(3, new BalanceSettings { SlownessIntensity = 5 }); Check(p.Slowness <= 5);
        });
        Test("Evolution cannot increase a short valid cooldown", () =>
        {
            var p = Grow(7, new BalanceSettings { EatCooldown = 1.3f }); Equal(1.3f, p.Cooldown);
        });
        Test("Default cooldown changes from 3 to 2 seconds", () => { Equal(3, Grow(6).Cooldown); Equal(2, Grow(7).Cooldown); });
        Test("A role snapshots its configuration", () =>
        {
            var settings = new BalanceSettings(); var p = new BiteProgress(settings);
            settings.BaseHumeShield = 999; Equal(400, p.MaxShield);
        });
        Test("Negative shield gains are rejected", () => Invalid(new BalanceSettings { HumeShieldPerHit = -1 }));
        Test("Negative regeneration is rejected", () => Invalid(new BalanceSettings { HumeShieldRegenRate = -1 }));
        Test("Base HS above the limit is rejected", () => Invalid(new BalanceSettings { BaseHumeShield = 2500 }));
        Test("Nonfinite spawn chance is rejected", () => Invalid(new BalanceSettings { SpawnChance = float.NaN }));
        Test("Cooldown below client limit is rejected", () => Invalid(new BalanceSettings { EatCooldown = 0.5f }));
        Test("Descending evolution thresholds are rejected", () => Invalid(new BalanceSettings { EvolutionTwoHits = 2 }));
        Test("Damage reduction multiplier is rejected", () => Invalid(new BalanceSettings { EvolvedDamageMultiplier = 0.5f }));
        Test("Final movement cannot get worse", () => Invalid(new BalanceSettings { FinalEvolutionMovementBoost = 5 }));
        Console.WriteLine("PASS: " + passed + " regression cases.");
    }

    private static BiteProgress New() => new BiteProgress(new BalanceSettings());
    private static BiteProgress Grow(int bites, BalanceSettings settings = null)
    {
        var progress = new BiteProgress(settings ?? new BalanceSettings());
        for (int i = 1; i <= bites; i++) Check(progress.TryCommit(i, 50, true));
        return progress;
    }
    private static void Invalid(BalanceSettings settings) => Check(settings.ValidateBalance().Count != 0);
    private static void Test(string name, Action test)
    {
        try { test(); passed++; Console.WriteLine("PASS " + name); }
        catch (Exception ex) { Console.Error.WriteLine("FAIL " + name + ": " + ex.Message); Environment.Exit(1); }
    }
    private static void Equal(float expected, float actual)
    { if (Math.Abs(expected - actual) > 0.001f) throw new Exception("Expected " + expected + ", actual " + actual); }
    private static void Check(bool condition) { if (!condition) throw new Exception("Condition failed"); }
}

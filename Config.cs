using Exiled.API.Interfaces;
using System.ComponentModel;

namespace SCP153
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public string SchematicName { get; set; } = "SCP153";

        [Description("Szansa na pojawienie sie (0.1 = 10%).")]
        public float SpawnChance { get; set; } = 0.1f;

        [Description("Maksymalne bazowe HP dla SCP-153 (domyslnie 4500).")]
        public int MaxHealth { get; set; } = 4500;

        [Description("Poczatkowy HumeShield na starcie (domyslnie 300).")]
        public float BaseHumeShield { get; set; } = 300f;

        [Description("Maksymalny pułap HumeShielda, jaki Buli moze osiagnac (domyslnie 1500).")]
        public float MaxHumeShieldLimit { get; set; } = 1500f;

        [Description("Staly bonus do maksymalnego i aktualnego HumeShielda za kazde ugryzienie (+100).")]
        public float HumeShieldPerHit { get; set; } = 100f;

        [Description("Predkosc regeneracji HumeShielda na sekunde.")]
        public float HumeShieldRegenRate { get; set; } = 15f;

        [Description("Czas w sekundach bez otrzymywania obrazen potrzebny do rozpoczecia regeneracji tarczy.")]
        public float HumeShieldRegenDelay { get; set; } = 5f;

        [Description("Poczatkowa sila efektu Slowness (20).")]
        public byte SlownessIntensity { get; set; } = 20;

        [Description("Bazowe obrazenia ugryzienia przed 2 poziomem ewolucji.")]
        public float Damage { get; set; } = 50f;

        [Description("Bazowy czas odnowienia ugryzienia w sekundach.")]
        public float EatCooldown { get; set; } = 3f;
    }
}
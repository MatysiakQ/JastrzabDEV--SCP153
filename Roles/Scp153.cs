using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using MEC;
using PlayerRoles;
using UnityEngine;

namespace SCP153.Roles
{
    public class Scp153 : CustomRole
    {
        public override uint Id { get; set; } = 153;
        public override string Name { get; set; } = "SCP153";
        public override string Description { get; set; } = "Gowniak pozerajacy ofiary";
        public override string CustomInfo { get; set; } = "SCP-153 BULSON";
        public override RoleTypeId Role { get; set; } = RoleTypeId.Scp0492;
        public override int MaxHealth { get; set; } = 4500;
        public override float SpawnChance { get; set; } = 0f;

        protected override void RoleAdded(Player player)
        {
            if (player == null || !Check(player)) return;

            Timing.CallDelayed(0.8f, () =>
            {
                if (player == null || !player.IsAlive) return;

                var cfg = Plugin.Instance.Config;
                player.MaxHealth = cfg.MaxHealth;
                player.Health = cfg.MaxHealth;
                player.MaxHumeShield = cfg.BaseHumeShield;
                player.HumeShield = cfg.BaseHumeShield;

                player.Broadcast(14, "<size=32><color=red><b>JESTEŚ SCP-153 BULSON</b></color></size>\n<size=26><color=yellow>NIE DAJ IM ZJEŚĆ SWOJEGO KAJMAKU!</color></size>");
            });

            player.EnableEffect(EffectType.Fade, 255, 9999f);
            player.InfoArea = (PlayerInfoArea)0;

            if (Plugin.Instance.Config.SlownessIntensity > 0)
                player.EnableEffect(EffectType.Slowness, Plugin.Instance.Config.SlownessIntensity, 9999f);

            player.Scale = Vector3.one;

            if (!player.GameObject.TryGetComponent(out Scp153Component _))
                player.GameObject.AddComponent<Scp153Component>();

            base.RoleAdded(player);
        }

        protected override void RoleRemoved(Player player)
        {
            player.DisableEffect(EffectType.Fade);
            player.DisableEffect(EffectType.Slowness);
            player.DisableEffect(EffectType.MovementBoost);
            player.InfoArea = PlayerInfoArea.Nickname | PlayerInfoArea.CustomInfo | PlayerInfoArea.Badge | PlayerInfoArea.Role;
            player.Scale = Vector3.one;

            if (player.GameObject.TryGetComponent(out Scp153Component comp))
                Object.Destroy(comp);

            base.RoleRemoved(player);
        }
    }
}
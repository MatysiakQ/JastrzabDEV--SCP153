using Exiled.API.Features;
using Exiled.CustomRoles.API;
using SCP153.Logic;
using System;

namespace SCP153
{
    public class Plugin : Plugin<Config>
    {
        public static Plugin Instance;
        public override string Name => "SCP153";
        public override string Prefix => "scp153";
        public override string Author => "Matysiak";
        public override Version Version => new Version(1, 5, 0);

        public override void OnEnabled()
        {
            Instance = this;
            new Roles.Scp153().Register();
            LogicHandler.RegisterEvents();
            Log.Info("SCP153: Plugin zaladowany poprawnie!");
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            LogicHandler.UnregisterEvents();
            Exiled.CustomRoles.API.Features.CustomRole.UnregisterRoles();
            Instance = null;
            base.OnDisabled();
        }
    }
}
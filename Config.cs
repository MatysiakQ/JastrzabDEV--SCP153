using Exiled.API.Interfaces;
using SCP153.Logic;
using System.Collections.Generic;
using System.ComponentModel;

namespace SCP153
{
    public sealed class Config : BalanceSettings, IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; }
        public string SchematicName { get; set; } = "SCP153";
        [Description("Unikalne ID CustomRole; kolizja zatrzymuje plugin.")]
        public uint CustomRoleId { get; set; } = 153;

        public IReadOnlyList<string> Validate()
        {
            var errors = new List<string>(ValidateBalance());
            if (CustomRoleId == 0) errors.Add("custom_role_id musi byc wieksze od 0.");
            if (string.IsNullOrWhiteSpace(SchematicName)) errors.Add("schematic_name nie moze byc puste.");
            return errors;
        }
    }
}

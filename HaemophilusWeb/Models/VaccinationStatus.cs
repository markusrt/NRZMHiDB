using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum VaccinationStatus
    {
        [Description("Nein")] No = 0,
        [Description("Ja")] Yes = 1,
        [Description("keine Angabe")] NotStated = 2,
        [Description("Unbekannt")] Unknown = 3,
        [Description("nicht vollständig")] Incomplete = 4
    }
}
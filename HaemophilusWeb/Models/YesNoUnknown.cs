using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum YesNoUnknown
    {
        [Description("Nein")] No = 0,
        [Description("Ja")] Yes = 1,
        [Description("Unbekannt")] Unknown = 2,
    }
}
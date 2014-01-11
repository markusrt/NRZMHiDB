using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum YesNoUnknown
    {
        [Description("Unbekannt")]
        Unknown=0,
        [Description("Ja")]
        Yes=1,
        [Description("Nein")]
        No=2
    }
}
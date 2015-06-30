using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum YesNoOptional
    {
        [Description("Nein")]
        No = 0,
        [Description("Ja")]
        Yes = 1,
        [Description("keine Angabe")]
        NotStated = 2,
    }
}
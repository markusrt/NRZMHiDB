using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum YesNo
    {
        [Description("Nein")] No = 0,
        [Description("Ja")] Yes = 1,
    }
}
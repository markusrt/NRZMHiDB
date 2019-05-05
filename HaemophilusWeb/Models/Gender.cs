using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum Gender
    {
        [Description("männlich")]
        Male=0,
        [Description("weiblich")]
        Female=1,
        [Description("divers")]
        Intersex = 2,
        [Description("keine Angabe")]
        NotStated = 3
    }
}
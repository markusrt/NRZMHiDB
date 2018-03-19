using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum RkiStatus
    {
        [Description("kein Match")] None = 0,
        [Description("möglich")] Possible = 1,
        [Description("wahrscheinlich")] Probable = 2,
        [Description("definitiv")] Definite = 3
    }
}
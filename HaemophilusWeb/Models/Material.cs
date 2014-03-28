using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum Material
    {
        [Description("Blut")] Blood = 0,
        [Description("Liquor")] Liquor = 1,
        [Description("Andere")] Other = 2
    }
}
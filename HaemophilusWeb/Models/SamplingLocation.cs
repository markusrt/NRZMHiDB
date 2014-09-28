using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum SamplingLocation
    {
        [Description("Blut")] Blood = 0,
        [Description("Liquor")] Liquor = 1,
        [Description("Anderer")] Other = 2
    }
}
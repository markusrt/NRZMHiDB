using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum SamplingLocation
    {
        [Description("Blut")]
        [InvasiveSamplingLocation]
        Blood = 0,
        [Description("Liquor")]
        [InvasiveSamplingLocation]
        Liquor = 1,
        [Description("Anderer")] Other = 2
    }
}
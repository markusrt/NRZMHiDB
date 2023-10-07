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
        [Description("Anderer nicht invasiv")]
        OtherNonInvasive = 2,
        [Description("Anderer invasiv")]
        [InvasiveSamplingLocation]
        OtherInvasive = 3
    }

    public static class SamplingLocationExtension
    {
        public static bool IsOther(this SamplingLocation samplingLocation)
        {
            return samplingLocation == SamplingLocation.OtherNonInvasive ||
                   samplingLocation == SamplingLocation.OtherInvasive;
        }
    }
}
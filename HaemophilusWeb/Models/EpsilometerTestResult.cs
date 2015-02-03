using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum EpsilometerTestResult
    {
        [Description("sensibel")] Susceptible,
        [Description("intermediär")] Intermediate,
        [Description("resistent")] Resistant
    }
}
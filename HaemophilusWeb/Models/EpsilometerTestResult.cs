using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum EpsilometerTestResult
    {
        [Description("Sensibel")] Susceptible,
        [Description("Intermediär")] Intermediate,
        [Description("Resistent")] Resistant
    }
}
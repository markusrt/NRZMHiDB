using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum TestResult
    {
        [Description("Positiv")] Positive,
        [Description("Negativ")] Negative
    }
}
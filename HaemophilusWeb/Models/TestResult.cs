using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum TestResult
    {
        [Description("n.d.")] NotDetermined = 0,
        [Description("Negativ")] Negative = 1,
        [Description("Positiv")] Positive = 2
    }
}
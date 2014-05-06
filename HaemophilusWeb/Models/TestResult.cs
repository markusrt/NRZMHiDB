using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum TestResult
    {
        [Description("n.d.")] NotDetermined = 0,
        [Description("Negativ")] Negative = 1,
        [Description("Positiv")] Positive = 2
    }

    public enum SpecificTestResult
    {
        [Description("n.d.")] NotDetermined = 0,
        [Description("Negativ")] Negative = 1,
        [Description("H.influenzae")] HaemophilusInfluenzae = 2,
        [Description("H.haemolyticus")] HaemophilusHaemolyticus = 3,
        [Description("nicht auswertbar")] NotEvaluable = 4,
    }
}

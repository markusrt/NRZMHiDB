using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum TestResult
    {
        [Description("n.d.")] NotDetermined = 0,
        [Description("negativ")] Negative = 1,
        [Description("positiv")] Positive = 2
    }

    public enum UnspecificTestResult
    {
        [Description("n.d.")] NotDetermined = 0,
        [Description("durchgeführt")] Determined = 1,
    }

    public enum UnspecificOrNoTestResult
    {
        [Description("n.d.")] NotDetermined = 0,
        [Description("durchgeführt")] Determined = 1,
        [Description("kein Ergebnis")] NoResult = 2,
    }

    public enum SpecificTestResult
    {
        [Description("n.d.")] NotDetermined = 0,
        [Description("negativ")] Negative = 1,
        [Description("H. influenzae")] HaemophilusInfluenzae = 2,
        [Description("H. haemolyticus")] HaemophilusHaemolyticus = 3,
        [Description("H. parainfluenzae")] HaemophilusParainfluenzae = 4,
        [Description("nicht auswertbar")] NotEvaluable = 100,
    }

    public enum NativeMaterialTestResult
    {
        [Description("n.d.")] NotDetermined = 0,
        [Description("negativ")] Negative = 1,
        [Description("positiv")] Positive = 2,
        [Description("inhibitorisch")] Inhibitory = 3
    }
}
using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum FactorTest
    {
        [Description("n.d.")] NotDetermined = 0,
        V = 1,
        VX = 2,
        X = 3,
        [Description("Nicht beurteilbar")] NotEvaluable = 4,
        [Description("Kein Wachstum")] NoGrowth = 5
    }
}
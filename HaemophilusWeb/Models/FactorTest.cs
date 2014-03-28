using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum FactorTest
    {
        V = 0,
        VX = 1,
        X = 2,
        [Description("Nicht beurteilbar")] NotEvaluable = 3,
        [Description("Kein Wachstum")] NoGrowth = 4,
        [Description("n.d.")] NotDetermined = 5
    }
}
using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum FactorTest
    {
        [Description("n.d.")] NotDetermined = 0,
        V = 1,
        VX = 2,
        X = 3,
        [Description("V/VX")] VVX = 4,
        [Description("Nicht beurteilbar")] NotEvaluable = 5,
    }
}
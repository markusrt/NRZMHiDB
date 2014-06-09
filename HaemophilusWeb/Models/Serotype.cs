using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum Serotype
    {
        [Description("n.d.")] NotDetermined = 0,
        [Description("a")]
        A = 1,
        [Description("b")]
        B = 2,
        [Description("c")]
        C = 3,
        [Description("d")]
        D = 4,
        [Description("e")]
        E = 5,
        [Description("f")]
        F = 6,
        [Description("negativ")] Negative = 7,
    }
}
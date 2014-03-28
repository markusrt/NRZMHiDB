using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum Serotype
    {
        A = 0,
        B = 1,
        C = 2,
        D = 3,
        E = 4,
        F = 5,
        [Description("Negativ")] Negative = 6,
        [Description("n.d.")] NotDetermined = 7
    }
}
using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum Serotype
    {
        [Description("n.d.")] NotDetermined = 0,
        A = 1,
        B = 2,
        C = 3,
        D = 4,
        E = 5,
        F = 6,
        [Description("Negativ")] Negative = 7,
    }
}
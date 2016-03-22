using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum Antibiotic
    {
        Ampicillin = 0,
        [Description("Amoxicillin / Clavulansäure")] AmoxicillinClavulanate = 1,
        [Description("Cefotaxim")] Cefotaxime = 2,
        Meropenem = 3,
        Imipenem = 4,
        Ciprofloxacin = 5

    }
}
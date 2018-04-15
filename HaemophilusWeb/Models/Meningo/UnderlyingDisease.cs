using System.ComponentModel;

namespace HaemophilusWeb.Models.Meningo
{
    public enum UnderlyingDisease
    {
        [Description("Alkoholismus")] Alcoholism = 0,
        [Description("Tonsillitis")] Tonsillitis = 1,
        [Description("Asthma")] Asthma = 2,
        [Description("Pharyngitis")] Pharyngitis = 3,
        [Description("Sinusitis")] Sinusitis = 4,
        [Description("Bronchitis")] Bronchitis = 5,
        [Description("Pneumonie")] Pneumonia = 6,
        [Description("COPD")] ChronicObstructivePulmonaryDisease = 7,
        [Description("CysticFibrosis")] CF = 8,
        [Description("Otitis")] Otitis = 9,
        [Description("Andere")] Other = 32768
    }
}
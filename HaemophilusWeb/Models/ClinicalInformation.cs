using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum ClinicalInformation
    {
        [Description("k.A.")] NotAvailable = 0,
        [Description("Meningitis")] Meningitis = 1,
        [Description("Sepsis")] Sepsis = 2,
        [Description("Pneumonie")] Pneumonia = 3,
        [Description("Andere")] Other = 4
    }
}
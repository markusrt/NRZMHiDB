using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum ClinicalInformation
    {
        [Description("k.A.")]
        NotAvailable,
        [Description("Meningitis")]
        Meningitis,
        [Description("Sepsis")]
        Sepsis,
        [Description("Pneumonie")]
        Pneumonia,
        [Description("Andere")]
        Other
    }
}
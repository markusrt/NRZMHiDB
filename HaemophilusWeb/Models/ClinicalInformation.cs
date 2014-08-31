using System;
using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    [Flags]
    public enum ClinicalInformation
    {
        None = 0,
        [Description("k.A.")] NotAvailable = 1,
        [Description("Meningitis")] Meningitis = 2,
        [Description("Sepsis")] Sepsis = 4,
        [Description("Pneumonie")] Pneumonia = 8,
        [Description("Andere")] Other = 32768
    }
}
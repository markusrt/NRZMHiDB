using System;
using System.ComponentModel;

namespace HaemophilusWeb.Models.Meningo
{
    [Flags]
    public enum MeningoClinicalInformation
    {
        None = 0,
        [Description("k.A.")] NotAvailable = 1,
        [Description("Meningitis")] Meningitis = 2,
        [Description("Sepsis")] Sepsis = 4,
        [Description("WFS")] WaterhouseFriderichsenSyndrome = 8,
        [Description("Letal")] Lethal = 16,
        [Description("Keine Symptome")] NoSymptoms = 32,
        [Description("Unbekannt")] Unknown = 64,
        [Description("Geheilt")] Cured = 128,
        [Description("Andere")] Other = 32768
    }
}
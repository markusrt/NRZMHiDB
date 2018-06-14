using System;
using System.ComponentModel;

namespace HaemophilusWeb.Models.Meningo
{
    [Flags]
    public enum RiskFactors
    {
        None = 0,
        [Description("k.A.")] NotAvailable = 1,
        [Description("Asplenie")] Asplenia = 2,
        [Description("Komplementdefekt")] ComplementDefect = 4,
        [Description("Eculizumab-Therapie")] EculizumabTherapy = 8,
        [Description("Anderer")] Other = 32768
    }
}
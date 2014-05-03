using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum State
    {
        [Description("Unbekannt")]
        Unknown,
        [Description("Schleswig-Holstein")] SH = 1,
        [Description("Hamburg")] HH = 2,
        [Description("Niedersachsen")] NI = 3,
        [Description("Bremen")] HB = 4,
        [Description("Nordrhein-Westfalen")] NW = 5,
        [Description("Hessen")] HE = 6,
        [Description("Rheinland-Pfalz")] RP = 7,
        [Description("Baden-Württemberg")] BW = 8,
        [Description("Bayern")] BY = 9,
        [Description("Saarland")] SL = 10,
        [Description("Berlin")] BE = 11,
        [Description("Brandenburg")] BB = 12,
        [Description("Mecklenburg-Vorpommern")] MV = 13,
        [Description("Sachsen")] SN = 14,
        [Description("Sachsen-Anhalt")] ST = 15,
        [Description("Thüringen")] TH = 16,
        [Description("Ausland")] Overseas = 17,
    }
}
using System;
using System.ComponentModel;

namespace HaemophilusWeb.Models.Meningo
{
    [Flags]
    public enum Epidemiology
    {
        None = 0,
        [Description("k.A.")] NotAvailable = 1,
        [Description("Einzelerkrankung")] SingleDisease = 2,
        [Description("Keimträger")] GermCarrier = 4,
        [Description("Cluster")] Cluster = 8,
        [Description("Umgebungsuntersuchung")] ContactInvestigation = 16,
    }
}
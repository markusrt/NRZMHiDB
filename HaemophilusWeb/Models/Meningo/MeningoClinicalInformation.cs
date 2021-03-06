﻿using System;
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
        [Description("Keine Symptome")] NoSymptoms = 16,
        [Description("Andere")] Other = 32768
    }
}
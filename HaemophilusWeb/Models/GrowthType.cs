using System;
using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    [Flags]
    public enum GrowthType
    {
        None = 0,
        [Description("Wachstum auf Blut")] GrowthOnBlood = 1,
        [Description("Typisches Wachstum auf KB")] TypicalGrowthOnKb = 2,
        [Description("Atypisches Wachstum auf KB")] AtypicalGrowthOnKb = 4
    }
}
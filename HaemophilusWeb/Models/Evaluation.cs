using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum Evaluation
    {
        [Description("NTHi")] HaemophilusNonEncapsulated = 0,
        [Description("Hia")] HaemophilusTypeA = 1,
        [Description("Hib")] HaemophilusTypeB = 2,
        [Description("Hic")] HaemophilusTypeC = 3,
        [Description("Hid")] HaemophilusTypeD = 4,
        [Description("Hie")] HaemophilusTypeE = 5,
        [Description("Hif")] HaemophilusTypeF = 6,
        [Description("H.haemolyticus")] HaemophilusHemolyticus = 7,
        [Description("H.parainfluenzae")] HaemophilusParainfluenzae = 8,
        [Description("kein Wachstum")] NoGrowth = 9,
        [Description("keine Haemophilus-Spezies")] NoHaemophilusSpecies = 10,
        [Description("H.influenzae")] HaemophilusInfluenzae = 11,
        [Description("Haemophilus sp., nicht H.influenzae")] HaemophilusSpeciesNoHaemophilusInfluenzae = 12,
        [Description("kein H.influenzae")] NoHaemophilusInfluenzae = 13,
    }
}
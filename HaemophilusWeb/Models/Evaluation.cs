using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum Evaluation
    {
        [Description("NTHi")] HaemophilusNonEncapsulated,
        [Description("Hia")] HaemophilusTypeA,
        [Description("Hib")] HaemophilusTypeB,
        [Description("Hic")] HaemophilusTypeC,
        [Description("Hid")] HaemophilusTypeD,
        [Description("Hie")] HaemophilusTypeE,
        [Description("Hif")] HaemophilusTypeF,
        [Description("H. haemolyticus")] HaemophilusHemolyticus,
        [Description("H. parainfluenzae")] HaemophilusParainfluenzae,
        [Description("kein Wachstum")] NoGrowth,
        [Description("keine Haemophilus Spezies")] NoHaemophilusSpecies,
    }
}
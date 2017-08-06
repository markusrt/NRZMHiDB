using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum MeningoMaterial
    {
        [Description("Vitaler Stamm")] VitalStem = 0,
        [Description("Nicht angewachsen")] NoGrowth = 1,
        [Description("Nativmaterial")] NativeMaterial = 2,
        [Description("Hitzeinaktiviert")] HeatInactived = 3
    }
}
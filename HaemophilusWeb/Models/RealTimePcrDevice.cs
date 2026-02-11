using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum RealTimePcrDevice
    {
        [Description("n.d.")] NotDetermined = 0,
        [Description("NHS Meningitis Real Tm, Firma Sacace")] Sacace = 1,
        [Description("BD MAX")] BdMax = 2
    }
}

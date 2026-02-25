using System.ComponentModel;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Models
{
    public enum RealTimePcrDevice
    {
        [Description(EnumEditor.HiddenOnUserInterface)] None = 0,
        [Description("NSH Real-Time-PCR (BD-MAX)")] NshBdMax = 1,
        [Description("NHS Real-Time-PCR (Sacace)")] NhsSacace = 2,
    }
}

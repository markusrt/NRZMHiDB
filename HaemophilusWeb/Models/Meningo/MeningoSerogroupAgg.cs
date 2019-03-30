using System.ComponentModel;

namespace HaemophilusWeb.Models.Meningo
{
    public enum MeningoSerogroupAgg
    {
        [Description("n.d.")] NotDetermined = 0,
        [Description("A")] A = 1,
        [Description("B")] B = 2,
        [Description("C")] C = 3,
        [Description("E")] E = 4,
        [Description("W")] W = 5,
        [Description("X")] X = 6,
        [Description("Y")] Y = 7,
        [Description("Z")] Z = 8,
        [Description("W/Y")] WY = 9,
        [Description("negativ")] Negative = 10,
        [Description("auto")] Auto = 11,
        [Description("poly")] Poly = 12,
        [Description("cnl")] Cnl = 13
    }
}
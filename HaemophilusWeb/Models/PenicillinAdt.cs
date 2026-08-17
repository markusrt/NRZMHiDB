using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum PenicillinAdt
    {
        [Description("n.d.")] NotDetermined = 0,
        [Description("1")] One = 1,
        [Description("2")] Two = 2,
        [Description("3")] Three = 3,
        [Description("4")] Four = 4,
        [Description("5")] Five = 5,
        [Description("6")] Six = 6,
        [Description("7")] Seven = 7,
        [Description("8")] Eight = 8,
        [Description("9")] Nine = 9,
        [Description("10")] Ten = 10,
        [Description("11")] Eleven = 11,
        [Description("≥ 12")] GreaterThanTwelve = 12,
    }
}

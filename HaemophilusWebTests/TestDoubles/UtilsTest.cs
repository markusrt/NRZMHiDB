using System.ComponentModel;

namespace HaemophilusWeb.TestDoubles
{
    public enum UtilsTest
    {
        [Description("Null")] Zero = 0,
        [Mock] [Description("Eins")] One = 1,
        Two = 2
    }
}
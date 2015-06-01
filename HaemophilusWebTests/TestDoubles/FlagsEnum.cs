using System;
using System.ComponentModel;

namespace HaemophilusWeb.TestDoubles
{
    [Flags]
    public enum FlagsEnum
    {
        [Description("Null")] Zero = 0,
        [Description("Eins")] One = 1,
        [Description("Zwei")] Two = 2,
        Four = 4,
    }
}
using System;
using System.ComponentModel;

namespace HaemophilusWeb.Models.Meningo
{
    [Flags]
    public enum RealTimePcrResult
    {
        [Description("Neisseria meningitidis")]
        NeisseriaMeningitidis = 1,
        [Description("Haemophilus influenzae")]
        HaemophilusInfluenzae = 2,
        [Description("Streptococcus pneumoniae")]
        StreptococcusPneumoniae = 4,
    }
}
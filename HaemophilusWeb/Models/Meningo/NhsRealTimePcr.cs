using System.ComponentModel;

namespace HaemophilusWeb.Models.Meningo
{
    public enum NhsRealTimePcr
    {
        [Description("n.d.")]
        NotDetermined = 0,
        [Description("Haemophilus influenzae")]
        HaemophilusInfluenzae = 1,
        [Description("Neisseria meningitidis")]
        NeisseriaMeningitidis =2,
        [Description("Streptococcus pneumoniae")]
        StreptococcusPneumoniae = 3,
        [Description("negative")]
        Negative =4
    }
}
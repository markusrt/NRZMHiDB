using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum ReportStatus
    {
        [Description("Kein Befund erstellt")] None = 0,
        [Description("Teilbefund")] Preliminary = 1,
        [Description("Endbefund")] Final = 2,
    }
}
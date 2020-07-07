using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum DatabaseType
    {
        [Description("Haemophilus")]
        Haemophilus = 0,
        [Description("Meningokokken")]
        Meningococci = 1,
        [Description("Allgemein")]
        None = 2
    }
}
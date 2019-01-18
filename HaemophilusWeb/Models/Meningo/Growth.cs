using System.ComponentModel;

namespace HaemophilusWeb.Models.Meningo
{
    public enum Growth
    {
        [Description("Nein")] No = 0,
        [Description("typisches Wachstum")] TypicalGrowth = 1,
        [Description("atypisches Wachstum")] ATypicalGrowth = 2,
    }
}
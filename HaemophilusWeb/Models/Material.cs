using System.ComponentModel;

namespace HaemophilusWeb.Models
{
    public enum Material
    {
        [Description("Isolat")] Isolate = 0,
        [Description("Isolierte DNA")] IsolatedDna = 1,
        [Description("Nativmaterial")] NativeMaterial
    }
}
using HaemophilusWeb.Models;

namespace HaemophilusWeb.Utils
{
    public static class MeningoMaterialExtensions
    {
        public static bool IsNativeMaterial(this MeningoMaterial material)
        {
            return material == MeningoMaterial.NativeMaterial || material == MeningoMaterial.IsolatedDna;
        }
    }
}
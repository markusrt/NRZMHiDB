using HaemophilusWeb.Models;

namespace HaemophilusWeb.Utils;

public static class MeningoMaterialExtensions
{
    public static bool IsNativeMaterial(this MeningoMaterial material)
    {
        return material is MeningoMaterial.NativeMaterial or MeningoMaterial.IsolatedDna;
    }
}
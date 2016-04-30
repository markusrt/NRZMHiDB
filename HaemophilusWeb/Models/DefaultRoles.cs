using System.Collections;
using System.Collections.Generic;

namespace HaemophilusWeb.Models
{
    public static class DefaultRoles
    {
        public const string Administrator = "Administrator";
        public const string PublicHealth = "RKI";
        public const string User = "Standardbenutzer";

        public static IEnumerable<string> GetAll()
        {
            yield return User;
            yield return PublicHealth;
            yield return Administrator;
        }
    }
}
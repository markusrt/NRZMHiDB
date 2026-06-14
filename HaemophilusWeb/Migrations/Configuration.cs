using System.Data.Entity.Migrations;
using System.Linq;
using HaemophilusWeb.Models;
using HaemophilusWeb.Services;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;

namespace HaemophilusWeb.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
            ContextKey = "HaemophilusWeb.Models.ApplicationDbContext";
        }

        protected override void Seed(ApplicationDbContext context)
        {
            SeedConfigurationEntry(context, ConfigurationService.Keys.ReportSigners,
                new[] { "Dr. Jane Doe", "Dr. John Doe" });

            SeedConfigurationEntry(context, ConfigurationService.Keys.Announcement,
                new AnnouncementSetting { Text = string.Empty });

            SeedConfigurationEntry(context, ConfigurationService.Keys.LabDirector,
                "PD Dr. rer. nat. Vorname Nachname");

            SeedConfigurationEntry(context, ConfigurationService.Keys.MedicalDirector,
                "PD Dr. med. Vorname Nachname");

            SeedConfigurationEntry(context, ConfigurationService.Keys.Contacts,
                new[]
                {
                    "PD Dr. rer. nat. Vorname Nachname\nTelefon: 0931/31-XXXXX\nvorname.nachname@uni-wuerzburg.de",
                    "PD Dr. med. Vorname Nachname\nTelefon: 0931/31-XXXXX\nvorname.nachname@uni-wuerzburg.de",
                    "Dr. med. Vorname Nachname\nTelefon: 0931/31-XXXXX\nvorname.nachname@uni-wuerzburg.de"
                });
        }

        private static void SeedConfigurationEntry<T>(ApplicationDbContext context, string key, T value)
        {
            if (!context.ConfigurationEntries.Any(e => e.Key == key))
            {
                context.ConfigurationEntries.Add(
                    new ConfigurationEntry { Key = key, Value = JsonConvert.SerializeObject(value) });
            }
        }
    }
}

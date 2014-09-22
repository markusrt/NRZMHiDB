using System.Data.Entity.Migrations;

namespace HaemophilusWeb.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<HaemophilusWeb.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
            ContextKey = "HaemophilusWeb.Models.ApplicationDbContext";
        }

        protected override void Seed(HaemophilusWeb.Models.ApplicationDbContext context)
        {
        }
    }
}
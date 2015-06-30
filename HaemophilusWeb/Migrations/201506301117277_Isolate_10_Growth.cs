using System.Data.Entity.Migrations;

namespace HaemophilusWeb.Migrations
{
    public partial class Isolate_10_Growth : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Isolates", "Growth", c => c.Int(false, defaultValue: 2));
            AddColumn("dbo.Isolates", "TypeOfGrowth", c => c.Int(false));
        }

        public override void Down()
        {
            DropColumn("dbo.Isolates", "TypeOfGrowth");
            DropColumn("dbo.Isolates", "Growth");
        }
    }
}
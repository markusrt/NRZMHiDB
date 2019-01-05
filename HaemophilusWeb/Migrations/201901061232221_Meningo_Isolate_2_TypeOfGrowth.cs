namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Meningo_Isolate_2_TypeOfGrowth : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MeningoIsolates", "TypeOfGrowth", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MeningoIsolates", "TypeOfGrowth");
        }
    }
}

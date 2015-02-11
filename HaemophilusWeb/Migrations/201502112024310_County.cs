namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class County : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Counties",
                c => new
                    {
                        CountyId = c.Int(nullable: false, identity: true),
                        CountyNumber = c.String(),
                        Name = c.String(),
                        ValidSince = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.CountyId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Counties");
        }
    }
}

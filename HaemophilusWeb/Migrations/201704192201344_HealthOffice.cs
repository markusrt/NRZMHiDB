namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class HealthOffice : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.HealthOffices",
                c => new
                    {
                        HealthOfficeId = c.Int(nullable: false, identity: true),
                        Address = c.String(),
                        Email = c.String(),
                        Phone = c.String(),
                        Fax = c.String(),
                        PostalCode = c.String(),
                    })
                .PrimaryKey(t => t.HealthOfficeId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.HealthOffices");
        }
    }
}

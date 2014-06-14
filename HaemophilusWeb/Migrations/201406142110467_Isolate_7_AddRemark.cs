namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Isolate_7_AddRemark : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Isolates", "Remark", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Isolates", "Remark");
        }
    }
}

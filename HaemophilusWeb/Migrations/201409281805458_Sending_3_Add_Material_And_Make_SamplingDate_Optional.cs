namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Sending_3_Add_Material_And_Make_SamplingDate_Optional : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sendings", "Material", c => c.Int(nullable: false));
            AlterColumn("dbo.Sendings", "SamplingDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Sendings", "SamplingDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.Sendings", "Material");
        }
    }
}

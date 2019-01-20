namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EucastClinicalBreakpoint_Add_ValidFor : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EucastClinicalBreakpoints", "ValidFor", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EucastClinicalBreakpoints", "ValidFor");
        }
    }
}

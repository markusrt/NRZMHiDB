namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MeningoPatient_2_UnderlyingDisease : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MeningoPatients", "OtherUnderlyingDisease", c => c.String());
            AlterColumn("dbo.MeningoPatients", "UnderlyingDisease", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.MeningoPatients", "UnderlyingDisease", c => c.String());
            DropColumn("dbo.MeningoPatients", "OtherUnderlyingDisease");
        }
    }
}

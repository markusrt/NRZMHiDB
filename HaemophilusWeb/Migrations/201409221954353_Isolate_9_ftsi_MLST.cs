namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Isolate_9_ftsi_MLST : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Isolates", "Ftsi", c => c.Int(nullable: false));
            AddColumn("dbo.Isolates", "FtsiEvaluation1", c => c.String());
            AddColumn("dbo.Isolates", "FtsiEvaluation2", c => c.String());
            AddColumn("dbo.Isolates", "Mlst", c => c.Int(nullable: false));
            AddColumn("dbo.Isolates", "MlstSequenceType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Isolates", "MlstSequenceType");
            DropColumn("dbo.Isolates", "Mlst");
            DropColumn("dbo.Isolates", "FtsiEvaluation2");
            DropColumn("dbo.Isolates", "FtsiEvaluation1");
            DropColumn("dbo.Isolates", "Ftsi");
        }
    }
}

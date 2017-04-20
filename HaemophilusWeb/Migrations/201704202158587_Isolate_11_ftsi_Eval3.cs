namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Isolate_11_ftsi_Eval3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Isolates", "FtsiEvaluation3", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Isolates", "FtsiEvaluation3");
        }
    }
}

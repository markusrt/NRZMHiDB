namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NeisseriaPubMlstIsolate_ReactivityFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NeisseriaPubMlstIsolates", "BexseroReactivity", c => c.String());
            AddColumn("dbo.NeisseriaPubMlstIsolates", "TrumenbaReactivity", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.NeisseriaPubMlstIsolates", "TrumenbaReactivity");
            DropColumn("dbo.NeisseriaPubMlstIsolates", "BexseroReactivity");
        }
    }
}

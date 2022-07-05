namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NeisseriaPubMlstIsolate_Database : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NeisseriaPubMlstIsolates", "Database", c => c.String());
            Sql("UPDATE dbo.NeisseriaPubMlstIsolates SET [Database] = 'pubmlst_neisseria_isolates'");
        }
        
        public override void Down()
        {
            DropColumn("dbo.NeisseriaPubMlstIsolates", "Database");
        }
    }
}

namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// In order to revert this migration the data in NeisseriaPubMlstIsolates needs to be deleted
    /// and eventually re-imported afterwards. Also all references from MeningoIsolates need to be
    /// removed. This is due to the fact that the old database model had an issue where it was actually
    /// never possible to store a relation between MeningoIsolate and NeisseriaPubMlstIsolate
    /// </summary>
    public partial class NeisseriaPubMlstIsolate_AddPubMlstIdAndFixReference : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.NeisseriaPubMlstIsolates", "NeisseriaPubMlstIsolateId", "dbo.MeningoIsolates");
            DropIndex("dbo.NeisseriaPubMlstIsolates", new[] { "NeisseriaPubMlstIsolateId" });
            AddColumn("dbo.MeningoIsolates", "NeisseriaPubMlstIsolate_NeisseriaPubMlstIsolateId", c => c.Int());
            AddColumn("dbo.NeisseriaPubMlstIsolates", "PubMlstId", c => c.Int(nullable: false));
            CreateIndex("dbo.MeningoIsolates", "NeisseriaPubMlstIsolate_NeisseriaPubMlstIsolateId");
            AddForeignKey("dbo.MeningoIsolates", "NeisseriaPubMlstIsolate_NeisseriaPubMlstIsolateId", "dbo.NeisseriaPubMlstIsolates", "NeisseriaPubMlstIsolateId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MeningoIsolates", "NeisseriaPubMlstIsolate_NeisseriaPubMlstIsolateId", "dbo.NeisseriaPubMlstIsolates");
            DropIndex("dbo.MeningoIsolates", new[] { "NeisseriaPubMlstIsolate_NeisseriaPubMlstIsolateId" });
            DropColumn("dbo.NeisseriaPubMlstIsolates", "PubMlstId");
            DropColumn("dbo.MeningoIsolates", "NeisseriaPubMlstIsolate_NeisseriaPubMlstIsolateId");
            CreateIndex("dbo.NeisseriaPubMlstIsolates", "NeisseriaPubMlstIsolateId");
            AddForeignKey("dbo.NeisseriaPubMlstIsolates", "NeisseriaPubMlstIsolateId", "dbo.MeningoIsolates", "MeningoSendingId");
        }
    }
}

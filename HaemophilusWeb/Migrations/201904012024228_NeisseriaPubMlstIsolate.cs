namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NeisseriaPubMlstIsolate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NeisseriaPubMlstIsolates",
                c => new
                    {
                        NeisseriaPubMlstIsolateId = c.Int(nullable: false, identity: true),
                        PorAVr1 = c.String(),
                        PorAVr2 = c.String(),
                        FetAVr = c.String(),
                        PorB = c.String(),
                        Fhbp = c.String(),
                        Nhba = c.String(),
                        NadA = c.String(),
                        PenA = c.String(),
                        GyrA = c.String(),
                        ParC = c.String(),
                        ParE = c.String(),
                        RpoB = c.String(),
                        RplF = c.String(),
                        SequenceType = c.String(),
                        ClonalComplex = c.String(),
                    })
                .PrimaryKey(t => t.NeisseriaPubMlstIsolateId)
                .ForeignKey("dbo.MeningoIsolates", t => t.NeisseriaPubMlstIsolateId)
                .Index(t => t.NeisseriaPubMlstIsolateId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NeisseriaPubMlstIsolates", "NeisseriaPubMlstIsolateId", "dbo.MeningoIsolates");
            DropIndex("dbo.NeisseriaPubMlstIsolates", new[] { "NeisseriaPubMlstIsolateId" });
            DropTable("dbo.NeisseriaPubMlstIsolates");
        }
    }
}

using System.Data.Entity.Migrations;

namespace HaemophilusWeb.Migrations
{
    public partial class Isolate_2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EpsilometerTests",
                c => new
                {
                    EpsilometerTestId = c.Int(false, true),
                    EucastClinicalBreakpointId = c.Int(false),
                    Measurement = c.Single(false),
                    Result = c.Int(false),
                    Isolate_SendingId = c.Int(),
                })
                .PrimaryKey(t => t.EpsilometerTestId)
                .ForeignKey("dbo.EucastClinicalBreakpoints", t => t.EucastClinicalBreakpointId, true)
                .ForeignKey("dbo.Isolates", t => t.Isolate_SendingId)
                .Index(t => t.EucastClinicalBreakpointId)
                .Index(t => t.Isolate_SendingId);

            CreateTable(
                "dbo.EucastClinicalBreakpoints",
                c => new
                {
                    EucastClinicalBreakpointId = c.Int(false, true),
                    Antibiotic = c.Int(false),
                    AntibioticDetails = c.String(false, 128),
                    Version = c.String(false, 64),
                    ValidFrom = c.DateTime(false),
                    MicBreakpointSusceptible = c.Single(false),
                    MicBreakpointResistent = c.Single(false),
                })
                .PrimaryKey(t => t.EucastClinicalBreakpointId);
            CreateIndex("dbo.EucastClinicalBreakpoints", new[] { "AntibioticDetails", "Version" }, true,
                "IX_EucastClinicalBreakpointVersionAntibioticDetails");

            AddColumn("dbo.Isolates", "FactorTest", c => c.Int());
            AddColumn("dbo.Isolates", "Agglutination", c => c.Int());
            AddColumn("dbo.Isolates", "BetaLactamase", c => c.Int());
            AddColumn("dbo.Isolates", "Oxidase", c => c.Int());
            AddColumn("dbo.Isolates", "OuterMembraneProteinP2", c => c.Int());
            AddColumn("dbo.Isolates", "FuculoKinase", c => c.Int(false));
            AddColumn("dbo.Isolates", "OuterMembraneProteinP6", c => c.Int(false));
            AddColumn("dbo.Isolates", "BexA", c => c.Int());
            AddColumn("dbo.Isolates", "SerotypePcr", c => c.Int());
            AddColumn("dbo.Isolates", "RibosomalRna16S", c => c.Int(false));
            AddColumn("dbo.Isolates", "ApiNh", c => c.Int());
            AlterColumn("dbo.Patients", "Initials", c => c.String());
            AlterColumn("dbo.Patients", "Gender", c => c.Int());
        }

        public override void Down()
        {
            DropForeignKey("dbo.EpsilometerTests", "Isolate_SendingId", "dbo.Isolates");
            DropForeignKey("dbo.EpsilometerTests", "EucastClinicalBreakpointId", "dbo.EucastClinicalBreakpoints");
            DropIndex("dbo.EpsilometerTests", new[] {"Isolate_SendingId"});
            DropIndex("dbo.EpsilometerTests", new[] {"EucastClinicalBreakpointId"});
            DropIndex("dbo.EucastClinicalBreakpoints", "IX_EucastClinicalBreakpointVersionAntibioticDetails");
            AlterColumn("dbo.Patients", "Gender", c => c.Int(false));
            AlterColumn("dbo.Patients", "Initials", c => c.String(false));
            DropColumn("dbo.Isolates", "ApiNh");
            DropColumn("dbo.Isolates", "RibosomalRna16S");
            DropColumn("dbo.Isolates", "SerotypePcr");
            DropColumn("dbo.Isolates", "BexA");
            DropColumn("dbo.Isolates", "OuterMembraneProteinP6");
            DropColumn("dbo.Isolates", "FuculoKinase");
            DropColumn("dbo.Isolates", "OuterMembraneProteinP2");
            DropColumn("dbo.Isolates", "Oxidase");
            DropColumn("dbo.Isolates", "BetaLactamase");
            DropColumn("dbo.Isolates", "Agglutination");
            DropColumn("dbo.Isolates", "FactorTest");
            DropTable("dbo.EucastClinicalBreakpoints");
            DropTable("dbo.EpsilometerTests");
        }
    }
}
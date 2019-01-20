namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EucastClinicalBreakpoint_Update_Index : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.EucastClinicalBreakpoints", "IX_EucastClinicalBreakpointVersionAntibioticDetails");
            CreateIndex("dbo.EucastClinicalBreakpoints", new[] { "Antibiotic", "AntibioticDetails", "ValidFor", "Version" }, true,
                "IX_EucastClinicalBreakpointVersionAntibioticDetails");
        }
        
        public override void Down()
        {
            DropIndex("dbo.EucastClinicalBreakpoints", "IX_EucastClinicalBreakpointVersionAntibioticDetails");
            CreateIndex("dbo.EucastClinicalBreakpoints", new[] { "Antibiotic", "AntibioticDetails", "Version" }, true,
                "IX_EucastClinicalBreakpointVersionAntibioticDetails");
        }
    }
}

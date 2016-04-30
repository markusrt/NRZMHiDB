namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EucastClinicalBreakpoint_Make_Some_Fields_Optional : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.EucastClinicalBreakpoints", "IX_EucastClinicalBreakpointVersionAntibioticDetails");

            AlterColumn("dbo.EucastClinicalBreakpoints", "AntibioticDetails", c => c.String(maxLength: 128));
            AlterColumn("dbo.EucastClinicalBreakpoints", "Version", c => c.String(maxLength: 64));
            AlterColumn("dbo.EucastClinicalBreakpoints", "ValidFrom", c => c.DateTime());
            AlterColumn("dbo.EucastClinicalBreakpoints", "MicBreakpointSusceptible", c => c.Single());
            AlterColumn("dbo.EucastClinicalBreakpoints", "MicBreakpointResistent", c => c.Single());

            CreateIndex("dbo.EucastClinicalBreakpoints", new[] { "Antibiotic", "AntibioticDetails", "Version" }, true,
                "IX_EucastClinicalBreakpointVersionAntibioticDetails");
        }
        
        public override void Down()
        {
            AlterColumn("dbo.EucastClinicalBreakpoints", "MicBreakpointResistent", c => c.Single(nullable: false));
            AlterColumn("dbo.EucastClinicalBreakpoints", "MicBreakpointSusceptible", c => c.Single(nullable: false));
            AlterColumn("dbo.EucastClinicalBreakpoints", "ValidFrom", c => c.DateTime(nullable: false));
            AlterColumn("dbo.EucastClinicalBreakpoints", "Version", c => c.String(nullable: false, maxLength: 64));
            AlterColumn("dbo.EucastClinicalBreakpoints", "AntibioticDetails", c => c.String(nullable: false, maxLength: 128));
        }
    }
}

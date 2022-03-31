namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReorderSpecificTestResult : DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE dbo.Isolates SET OuterMembraneProteinP6 = 100 WHERE OuterMembraneProteinP6 = 4");
        }
        
        public override void Down()
        {
        }
    }
}

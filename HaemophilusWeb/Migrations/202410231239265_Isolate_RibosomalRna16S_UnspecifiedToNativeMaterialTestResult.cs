namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Isolate_RibosomalRna16S_UnspecifiedToNativeMaterialTestResult : DbMigration
    {
        public override void Up()
        {
            // Change enum from UnspecificTestResult to NativeMaterialTestResult
            Sql("UPDATE dbo.Isolates SET RibosomalRna16S = 2 WHERE RibosomalRna16S = 1");
        }
        
        public override void Down()
        {
        }
    }
}

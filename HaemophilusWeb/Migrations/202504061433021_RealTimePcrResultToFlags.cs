namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RealTimePcrResultToFlags : DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE dbo.MeningoIsolates SET RealTimePcrResult = 4 Where RealTimePcrResult = 3");
            Sql("UPDATE dbo.Isolates SET RealTimePcrResult = 4 Where RealTimePcrResult = 3");
        }
        
        public override void Down()
        {
            Sql("UPDATE dbo.MeningoIsolates SET RealTimePcrResult = 3 Where RealTimePcrResult = 4");
            Sql("UPDATE dbo.Isolates SET RealTimePcrResult = 3 Where RealTimePcrResult = 4");
        }
    }
}

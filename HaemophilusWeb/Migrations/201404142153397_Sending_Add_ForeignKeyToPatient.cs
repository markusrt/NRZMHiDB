namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Sending_Add_ForeignKeyToPatient : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Sendings", "PatientId");
            AddForeignKey("dbo.Sendings", "PatientId", "dbo.Patients", "PatientId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Sendings", "PatientId", "dbo.Patients");
            DropIndex("dbo.Sendings", new[] { "PatientId" });
        }
    }
}

using System.Data.Entity.Migrations;

namespace HaemophilusWeb.Migrations
{
    public partial class Sending_2_Rename_Material_To_SamplingLocation : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.Sendings", "Material", "SamplingLocation");
            RenameColumn("dbo.Sendings", "OtherMaterial", "OtherSamplingLocation");
        }

        public override void Down()
        {
            RenameColumn("dbo.Sendings", "SamplingLocation", "Material");
            RenameColumn("dbo.Sendings", "OtherSamplingLocation", "OtherMaterial");
        }
    }
}
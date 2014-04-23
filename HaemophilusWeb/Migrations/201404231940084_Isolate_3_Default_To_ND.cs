namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Isolate_3_Default_To_ND : DbMigration
    {
        public override void Up()
        {
            ChangeColumnFromNullableIntToIntDefaultingToZero("FactorTest");
            ChangeColumnFromNullableIntToIntDefaultingToZero("Agglutination");
            ChangeColumnFromNullableIntToIntDefaultingToZero("BetaLactamase");
            ChangeColumnFromNullableIntToIntDefaultingToZero("Oxidase");
            ChangeColumnFromNullableIntToIntDefaultingToZero("OuterMembraneProteinP2");
            ChangeColumnFromNullableIntToIntDefaultingToZero("BexA");
            ChangeColumnFromNullableIntToIntDefaultingToZero("SerotypePcr");
            ChangeColumnFromNullableIntToIntDefaultingToZero("ApiNh");
        }

        private void ChangeColumnFromNullableIntToIntDefaultingToZero(string column)
        {
            var sqlCommand = string.Format("Update dbo.Isolates set {0}=0 where {0} is null", column);
            Sql(sqlCommand);
            AlterColumn("dbo.Isolates", column, c => c.Int(nullable: false));
        }

        public override void Down()
        {
            AlterColumn("dbo.Isolates", "ApiNh", c => c.Int());
            AlterColumn("dbo.Isolates", "SerotypePcr", c => c.Int());
            AlterColumn("dbo.Isolates", "BexA", c => c.Int());
            AlterColumn("dbo.Isolates", "OuterMembraneProteinP2", c => c.Int());
            AlterColumn("dbo.Isolates", "Oxidase", c => c.Int());
            AlterColumn("dbo.Isolates", "BetaLactamase", c => c.Int());
            AlterColumn("dbo.Isolates", "Agglutination", c => c.Int());
            AlterColumn("dbo.Isolates", "FactorTest", c => c.Int());
        }
    }
}

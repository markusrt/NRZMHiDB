using System.Data.SqlClient;

namespace ScreenshotTests
{
    /// <summary>Creates a clean, empty test catalog so id-based routes (e.g. /Sender/Edit/1) are deterministic.</summary>
    internal static class LocalDb
    {
        private static string MasterConnectionString =>
            $@"Data Source={Paths.LocalDbInstance};Initial Catalog=master;Integrated Security=True";

        /// <summary>Drops the screenshot catalog if present so the next migration recreates it from scratch.</summary>
        public static void DropTestCatalog()
        {
            using (var connection = new SqlConnection(MasterConnectionString))
            {
                connection.Open();
                var sql = $@"
IF DB_ID('{Paths.TestCatalog}') IS NOT NULL
BEGIN
    ALTER DATABASE [{Paths.TestCatalog}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{Paths.TestCatalog}];
END";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}

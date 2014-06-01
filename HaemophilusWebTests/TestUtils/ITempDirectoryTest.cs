namespace HaemophilusWeb.TestUtils
{
    /// <summary>
    ///     Attaching this interface to a NUnit test class provides access
    ///     to a temporary directory. The directory is created and deleted
    ///     automatically for each test case.
    /// </summary>
    [TempDirectoryAwareAction]
    public interface ITempDirectoryTest
    {
        string TemporaryDirectoryToStoreTestData { get; set; }
    }
}
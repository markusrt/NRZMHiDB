using System.IO;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace HaemophilusWeb.TestUtils
{
    public class TempDirectoryAwareActionAttribute : TestActionAttribute
    {
        private DirectoryInfo tempDirectoryToStoreTestData;

        public override ActionTargets Targets
        {
            get { return ActionTargets.Test; }
        }

        public override void BeforeTest(ITest test)
        {
            var tempDirectoryTest = test.Fixture as ITempDirectoryTest;
            if (tempDirectoryTest == null)
            {
                return;
            }

            CreateTemporaryDirectoryAndAssignToTest(tempDirectoryTest);
        }

        public override void AfterTest(ITest test)
        {
            tempDirectoryToStoreTestData?.Delete(true);
        }

        private void CreateTemporaryDirectoryAndAssignToTest(ITempDirectoryTest tempDirectoryTest)
        {
            tempDirectoryToStoreTestData = CreateTemporaryDirectory();
            tempDirectoryTest.TemporaryDirectoryToStoreTestData = tempDirectoryToStoreTestData.FullName;
        }

        private static DirectoryInfo CreateTemporaryDirectory()
        {
            var tempFile = Path.GetTempFileName();
            File.Delete(tempFile);
            return Directory.CreateDirectory(tempFile);
        }
    }
}
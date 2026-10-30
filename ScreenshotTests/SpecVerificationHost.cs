using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ScreenshotTests
{
    /// <summary>
    /// Keeps the seeded app running for an interactive session (for example an agent that verifies the
    /// as-is specifications under <c>specs/</c> in a browser). The <see cref="Harness"/> set-up fixture
    /// has already reset and seeded the database, swapped the connection string and started IIS Express
    /// when this test begins; the test only publishes the connection details and waits.
    ///
    /// Run:   dotnet test ScreenshotTests\ScreenshotTests.csproj --no-build --filter SpecVerificationHost
    /// Stop:  create the file ScreenshotTests\host.stop (or wait for SPEC_VERIFY_TIMEOUT_MINUTES, default 240).
    /// Ready: ScreenshotTests\host.ready exists and contains the base URL and the seeded accounts.
    /// </summary>
    [TestFixture]
    [Explicit("Long-running host for interactive spec verification; not part of the regular test run.")]
    public class SpecVerificationHost
    {
        private static string HostDir => Path.Combine(Paths.RepoRoot, "ScreenshotTests");

        public static string ReadyFile => Path.Combine(HostDir, "host.ready");

        public static string StopFile => Path.Combine(HostDir, "host.stop");

        [Test]
        public async Task BootAndWait()
        {
            var timeoutMinutes = int.TryParse(
                Environment.GetEnvironmentVariable("SPEC_VERIFY_TIMEOUT_MINUTES"), out var parsed) ? parsed : 240;
            var deadline = DateTime.UtcNow.AddMinutes(timeoutMinutes);

            Seeder.SeedSpecVerificationFixtures();

            File.Delete(StopFile);
            File.WriteAllText(ReadyFile, ReadyInfo());
            TestContext.Progress.WriteLine(ReadyInfo());
            TestContext.Progress.WriteLine(
                $"Waiting up to {timeoutMinutes} min. Create '{StopFile}' to shut down.");

            try
            {
                while (DateTime.UtcNow < deadline && !File.Exists(StopFile))
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }
            finally
            {
                File.Delete(ReadyFile);
                File.Delete(StopFile);
            }

            TestContext.Progress.WriteLine("Spec verification host stopped.");
        }

        private static string ReadyInfo()
        {
            var singleRole = string.Join("\n", Seeder.SingleRoleUsers.Select(u => $"  {u.UserName} / {Seeder.Password}  ({u.Role})"));
            return
                $"BaseUrl: {Paths.BaseUrl}\n" +
                $"Catalog: {Paths.TestCatalog} on {Paths.LocalDbInstance}\n" +
                "Accounts:\n" +
                $"  {Seeder.UserName} / {Seeder.Password}  (all roles)\n" +
                singleRole + "\n" +
                "Seeded ids: Sender 1, Patient 1, Sending 1, Isolate 1, MeningoPatient 1, MeningoSending 1, MeningoIsolate 1\n" +
                $"Fixtures: sender {Seeder.FixtureSenderName} with KL002/20 (active), KL003/20 (soft-deleted), " +
                "MZ002/20 (active), MZ003/20 (soft-deleted), sampled 10.03.2020 / 15.06.2020 (013 FR-015, DEC-002)\n";
        }
    }
}

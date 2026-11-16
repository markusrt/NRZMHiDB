using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenshotTests
{
    /// <summary>Starts and stops IIS Express hosting the built HaemophilusWeb app for the harness.</summary>
    internal sealed class IisExpress : IDisposable
    {
        private Process _process;

        public bool HasExited => _process == null || _process.HasExited;

        /// <summary>
        /// Fails when something already listens on the port (for example a second spec verification host):
        /// IIS Express could not bind it, yet the readiness check would succeed against the other app.
        /// </summary>
        public static void EnsurePortIsFree()
        {
            var listeners = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
            if (listeners.Any(endpoint => endpoint.Port == Paths.Port))
            {
                throw new InvalidOperationException(
                    $"Port {Paths.Port} is already in use (another spec verification host or IIS Express?). " +
                    "Stop it first; the test catalog was not touched.");
            }
        }

        public void Start()
        {
            if (!System.IO.File.Exists(Paths.IisExpressExe))
            {
                throw new FileNotFoundExceptionWithHint(Paths.IisExpressExe);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = Paths.IisExpressExe,
                Arguments = $"/path:\"{Paths.WebAppDir}\" /port:{Paths.Port} /clr:v4.0 /systray:false",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            _process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
        }

        /// <summary>Waits until the app answers (first hit JIT-compiles views and can be slow).</summary>
        public async Task WaitUntilReadyAsync(TimeSpan timeout)
        {
            using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
            {
                var deadline = DateTime.UtcNow + timeout;
                Exception last = null;
                while (DateTime.UtcNow < deadline)
                {
                    if (_process.HasExited)
                    {
                        throw new InvalidOperationException($"IIS Express exited early with code {_process.ExitCode}.");
                    }

                    try
                    {
                        var response = await client.GetAsync(Paths.BaseUrl + "/Account/Login");
                        if ((int)response.StatusCode < 500)
                        {
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        last = ex;
                    }

                    await Task.Delay(1000);
                }

                throw new TimeoutException(
                    $"App at {Paths.BaseUrl} was not ready within {timeout.TotalSeconds:N0}s. Last error: {last?.Message}");
            }
        }

        public void Dispose()
        {
            try
            {
                if (_process != null && !_process.HasExited)
                {
                    _process.Kill();
                    _process.WaitForExit(10000);
                }
            }
            catch
            {
                // best effort teardown
            }
            finally
            {
                _process?.Dispose();
                _process = null;
            }
        }

        private sealed class FileNotFoundExceptionWithHint : Exception
        {
            public FileNotFoundExceptionWithHint(string path)
                : base($"IIS Express not found at '{path}'. Install IIS Express or adjust Paths.IisExpressExe.")
            {
            }
        }
    }
}

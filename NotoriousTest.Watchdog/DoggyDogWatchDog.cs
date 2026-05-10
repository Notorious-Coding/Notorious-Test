using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Watchdog;
using NotoriousTest.SqlLiteRegistry;

using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NotoriousTest.Watchdog
{
    public class DoggyDogWatchDog : IWatchDog
    {
        private readonly SqliteRegistryRepositoryConfiguration _registyConfiguration;
        private readonly ITestLogger _logger;
        private readonly DoggyDogWatchdogConfiguration _config;

        public DoggyDogWatchDog(SqliteRegistryRepositoryConfiguration registryConfiguration, ITestLogger logger, DoggyDogWatchdogConfiguration config)
        {
            _registyConfiguration = registryConfiguration;
            _logger = logger;
            _config = config;
        }

        public Process Start(Assembly currentAssembly, int currentPid, EnvironmentId environmentId, IEnumerable<string>? runtimePaths)
        {
            string assemblyPath = currentAssembly.Location;
            string? runtimesParams = runtimePaths == null ? null : string.Join("|", runtimePaths);

            if (_config.ManualLaunch)
                return WaitForDoggyDogToLaunch(currentPid, environmentId, assemblyPath, runtimesParams);
            return LaunchDoggyDog(currentPid, environmentId, assemblyPath, runtimesParams) ?? throw new Exception("Could not launch DoggyDog");
        }

        private Process? LaunchDoggyDog(int currentPid, EnvironmentId contextId, string assemblyPath, string? runtimesParams)
        {
            string watchdogPath = Path.Combine(AppContext.BaseDirectory, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "DoggyDog.Watchdog.exe" : "DoggyDog.Watchdog");

            string runtimeParameter = runtimesParams == null ? "" : $"--runtimes \"{runtimesParams}\" ";
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = watchdogPath,
                Arguments = $"--pid {currentPid} " +
                            $"--assembly \"{assemblyPath}\" " +
                            $"--connectionString \"{_registyConfiguration.ConnectionString}\" " +
                            $"--environment {contextId.Value} " +
                            runtimeParameter +
                            "--loglevel Info",
                UseShellExecute = true,
                CreateNoWindow = false,
            });


            return process;
        }

        private Process WaitForDoggyDogToLaunch(int currentPid, EnvironmentId environmentId, string assemblyPath, string? runtimesParams)
        {
            Environment.SetEnvironmentVariable("DOGGYDOG_DEBUG_PID", currentPid.ToString(), EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("DOGGYDOG_DEBUG_ASSEMBLY", assemblyPath, EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("DOGGYDOG_DEBUG_CONNECTIONSTRING", _registyConfiguration.ConnectionString, EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("DOGGYDOG_DEBUG_ENVIRONMENT", environmentId.Value.ToString(), EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("DOGGYDOG_DEBUG_RUNTIMES", runtimesParams, EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("DOGGYDOG_DEBUG_LOGLEVEL", "Debug", EnvironmentVariableTarget.User);

            Process? doggyDogProcess;
            do
            {
                _logger.Log("Waiting for DoggyDog.Watchdog to launch...", environmentId);
                doggyDogProcess = Process.GetProcessesByName("DoggyDog.Watchdog")?.FirstOrDefault();
                Thread.Sleep(5000);

            } while (doggyDogProcess == null);

            return doggyDogProcess;
        }

        public void SendSuccessSignal(EnvironmentId contextId) => File.WriteAllText(Path.Combine(Path.GetTempPath(), $"nt-{contextId.Value}.signal"), "OK");

        public static bool ReadSuccessSignal(EnvironmentId contextId)
        {
            string path = Path.Combine(Path.GetTempPath(), $"nt-{contextId.Value}.signal");
            bool isSuccess = File.Exists(path);
            if (isSuccess) File.Delete(path);

            return isSuccess;
        }
    }
}

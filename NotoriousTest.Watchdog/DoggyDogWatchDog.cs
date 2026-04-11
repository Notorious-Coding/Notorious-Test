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
        private readonly SqliteRegistryProviderConfiguration _registyConfiguration;
        private readonly ITestLogger _logger;

        public DoggyDogWatchDog(SqliteRegistryProviderConfiguration registryConfiguration, ITestLogger logger)
        {
            _registyConfiguration = registryConfiguration;
            _logger = logger;
        }

        public Process Start(Assembly currentAssembly, int currentPid, EnvironmentId contextId, IEnumerable<string>? runtimePaths)
        {
            var assemblyPath = currentAssembly.Location;
            string? runtimesParams = runtimePaths == null ? null : string.Join("|", runtimePaths);

#if (DEBUG)
            return WaitForDoggyDogToLaunch(currentPid, contextId, assemblyPath, runtimesParams);
#endif
            return LaunchDoggyDog(currentPid, contextId, assemblyPath, runtimesParams);
        }

        private Process LaunchDoggyDog(int currentPid, EnvironmentId contextId, string assemblyPath, string? runtimesParams)
        {
            var watchdogPath = Path.Combine(AppContext.BaseDirectory, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "DoggyDog.exe" : "DoggyDog");

            var runtimeParameter = runtimesParams == null ? "" : $"--runtimes \"{runtimesParams}\"";
            Process process = Process.Start(new ProcessStartInfo
            {
                FileName = watchdogPath,
                Arguments = $"--pid {currentPid} " +
                            $"--assembly \"{assemblyPath}\" " +
                            $"--connectionString \"{_registyConfiguration.ConnectionString}\" " +
                            $"--environment {contextId.Value} " +
                            runtimeParameter,
                UseShellExecute = true,
                CreateNoWindow = false,
            });


            return process;
        }

        private Process WaitForDoggyDogToLaunch(int currentPid, EnvironmentId contextId, string assemblyPath, string runtimesParams)
        {
            Environment.SetEnvironmentVariable("DEBUG_DOGGYDOG_PID", currentPid.ToString(), EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("DEBUG_DOGGYDOG_ASSEMBLY", assemblyPath, EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("DEBUG_DOGGYDOG_CS", _registyConfiguration.ConnectionString, EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("DEBUG_DOGGYDOG_ENVIRONMENT", contextId.Value.ToString(), EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("DEBUG_DOGGYDOG_RUNTIMES", runtimesParams, EnvironmentVariableTarget.User);

            Process? doggyDogProcess = null;
            do
            {
                _logger.Log("Waiting for DoggyDog to launch...");
                doggyDogProcess = Process.GetProcessesByName("DoggyDog")?.FirstOrDefault();
                Thread.Sleep(5000);

            } while (doggyDogProcess == null);

            return doggyDogProcess;
        }

        public void SendSuccessSignal(EnvironmentId contextId)
        {
            File.WriteAllText(Path.Combine(Path.GetTempPath(), $"nt-{contextId.Value}.signal"), "OK");
        }

        public static bool ReadSuccessSignal(EnvironmentId contextId)
        {
            var path = Path.Combine(Path.GetTempPath(), $"nt-{contextId.Value}.signal");
            var isSuccess = File.Exists(path);
            if (isSuccess) File.Delete(path);

            return isSuccess;
        }
    }
}
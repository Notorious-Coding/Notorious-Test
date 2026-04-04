using NotoriousTest.Core.Watchdog;
using NotoriousTest.SqlLiteRegistry;

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NotoriousTest.Watchdog
{
    public class DoggyDogWatchDog : IWatchDog
    {
        private readonly SqliteRegistryProviderConfiguration _registyConfiguration;

        public DoggyDogWatchDog(SqliteRegistryProviderConfiguration registryConfiguration)
        {
            _registyConfiguration = registryConfiguration;
        }

        public Process Start(Assembly currentAssembly, int currentPid)
        {
            var watchdogPath = Path.Combine(AppContext.BaseDirectory, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "DoggyDog.exe" : "DoggyDog");
            var assemblyPath = currentAssembly.Location;

            Process process = Process.Start(new ProcessStartInfo
            {
                FileName = watchdogPath,
                Arguments = $"--pid {currentPid} --assembly \"{assemblyPath}\" --connectionString \"{_registyConfiguration.ConnectionString}\"",
                UseShellExecute = true,
                CreateNoWindow = false,
            });


            return process;
        }

        public void SendSuccessSignal(int currentPid)
        {
            File.WriteAllText(Path.Combine(Path.GetTempPath(), $"nt-{currentPid}.signal"), "OK");
        }

        public static bool ReadSuccessSignal(int currentPid)
        {
            var path = Path.Combine(Path.GetTempPath(), $"nt-{currentPid}.signal");
            var isSuccess = File.Exists(path);
            if (isSuccess) File.Delete(path);

            return isSuccess;
        }
    }
}
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
        private readonly SqliteRegistryProviderConfiguration _configuration;

        public DoggyDogWatchDog(SqliteRegistryProviderConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Process Start(Assembly currentAssembly, int currentPid)
        {
            var watchdogPath = Path.Combine(AppContext.BaseDirectory, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "DoggyDog.exe" : "DoggyDog");
            var assemblyPath = currentAssembly.Location;

            Process process = Process.Start(new ProcessStartInfo
            {
                FileName = watchdogPath,
                Arguments = $"--pid {currentPid} --assembly \"{assemblyPath}\" --connectionString \"{_configuration.ConnectionString}\"",
                UseShellExecute = true,
                CreateNoWindow = false,
            });


            return process;
        }
    }
}

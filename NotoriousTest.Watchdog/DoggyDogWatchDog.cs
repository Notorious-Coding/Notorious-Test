using NotoriousTest.Core.Watchdog;

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace NotoriousTest.Watchdog
{
    public class DoggyDogWatchDog : IWatchDog
    {
        public Process Start(Assembly currentAssembly, int currentPid)
        {
            var watchdogPath = Path.Combine(AppContext.BaseDirectory, "NotoriousTest.DoggyDog.exe");
            var assemblyPath = currentAssembly.Location;

            return Process.Start(new ProcessStartInfo
            {
                FileName = watchdogPath,
                Arguments = $"--pid {currentPid} --assembly \"{assemblyPath}\"",
                UseShellExecute = true,
                CreateNoWindow = false,
            });
        }
    }
}

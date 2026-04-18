using DoggyDog.AssemblyLoader;
using DoggyDog.Logs;

using NotoriousTest.Core;
using NotoriousTest.Core.Infrastructures.Cleaner;
using NotoriousTest.Core.Registry;
using NotoriousTest.Watchdog;

using System.Diagnostics;
using System.Reflection;

namespace DoggyDog
{
    internal class DoggyDogRecoveryWatchdog
    {
        private const string BANNER = """
                                        |\     
                               \`-. _.._| \     
                                |_,'  __`. \    ██████╗  ██████╗  ██████╗  ██████╗ ██╗   ██╗    ██████╗  ██████╗  ██████╗ 
                                (.\ _/.| _  |   ██╔══██╗██╔═══██╗██╔════╝ ██╔════╝╚ ██╗ ██╔╝    ██╔══██╗██╔═══██╗██╔════╝ 
                               ,'      __ \ |   ██║  ██║██║   ██║██║  ███╗██║  ███╗ ╚████╔╝     ██║  ██║██║   ██║██║  ███╗
                             ,'     __/||\  |   ██║  ██║██║   ██║██║   ██║██║   ██║  ╚██╔╝      ██║  ██║██║   ██║██║   ██║
                            (..)  ,/|||||/  |   ██████╔╝╚██████╔╝╚██████╔╝╚██████╔╝   ██║       ██████╔╝╚██████╔╝╚██████╔╝
                               `-'_----   _.\   ╚═════╝  ╚═════╝  ╚═════╝  ╚═════╝    ╚═╝       ╚═════╝  ╚═════╝  ╚═════╝ 
                                  /`-._.-'_.-             -- .NET INTEGRATION TEST CRASH RECOVERY WATCHDOG --
                                  `-.__.-'
                                              
                        """;
        private readonly ITestAssemblyLoader _assemblyLoader;
        private readonly IRegistry _registry;
        private readonly Logger _logger;

        public DoggyDogRecoveryWatchdog(ITestAssemblyLoader assemblyLoader, IRegistry registry, Logger logger)
        {
            _assemblyLoader = assemblyLoader;
            _registry = registry;
            _logger = logger;
        }
        public static void Banner(int pid, Guid environmentId)
        {
            LogColorScope.DarkMagenta(() => Console.WriteLine(BANNER));
            var version = Assembly.GetExecutingAssembly().GetName().Version!;
            LogColorScope.DarkGray(() =>
            {
                Console.WriteLine($"                      Monitoring PID {pid}  ·  EID {environmentId}  ·  v{version.Major}.{version.Minor}.{version.Build}");
                Console.WriteLine();
            });
        }

        public async Task Run(int pid, Guid environmentId)
        {
            SetupRegistryHooks();

            using (var source = new CancellationTokenSource())
            {
                _logger.Debug("Watching registry for infrastructures updates.");
                Task watcherTask = Task.Run(() => _registry.Watch(environmentId, source.Token));

                _assemblyLoader.Load();
                _logger.Info($"Attached to test process with PID {pid} and EID {environmentId}");

                await WaitForTestProcess(pid);

                source.Cancel();
                await watcherTask;
            }

            if (DoggyDogWatchDog.ReadSuccessSignal(environmentId))
            {
                _logger.Trace($"Success signal found for {environmentId}.");
                _logger.Success($"Process {pid} for EID {environmentId} exited cleanly. No recovery needed.");
                Console.Read();
                Environment.Exit(0);
            }

            _logger.Error($"Process {pid} for EID {environmentId} exited abnormally. Initiating crash recovery...");

            IReadOnlyList<InfrastuctureRegistryEntry> entries = (await _registry.GetByEnvironmentId(new EnvironmentId(environmentId))).ToList();

            if (entries.Count == 0)
            {
                _logger.Warning($"No registered infrastructure found for EID {environmentId}. Nothing to clean up.");
                Console.Read();
                Environment.Exit(0);
            }

            _logger.Info($"{entries.Count} infrastructure(s) registered for EID {environmentId}. Starting cleanup...");

            foreach (InfrastuctureRegistryEntry entry in entries)
            {
                using var scope = _logger.CreateScope(entry.InfrastructureType.Name);

                IInfrastructureCleaner? cleaner = RetrieveCleaner(entry);

                if (cleaner != null)
                {
                    await cleaner.CleanAfterCrash(entry.EnvironmentId, entry.InfrastructureId, entry.Metadata);
                    _logger.Success("Cleanup successful.");
                    _logger.Debug("Removing registry entry...");
                    await _registry.Remove(entry.InfrastructureId);
                    _logger.Trace("Registry entry removed.");
                }
                else
                {
                    _logger.Error("Failed to instantiate cleaner. Skipping.");
                }
            }

            _logger.Success($"Crash recovery complete for PID {pid} and EID {environmentId}.");
            Console.Read();
            Environment.Exit(0);
        }

        private void SetupRegistryHooks()
        {
            _registry.OnInfrastructureReset += (entry) =>
            {
                using var scope = _logger.CreateScope(entry.InfrastructureType.Name);
                _logger.Info($"Reset has been triggered.");
            };

            _registry.OnInfrastructureCreated += (entry) =>
            {
                using var scope = _logger.CreateScope(entry.InfrastructureType.Name);
                _logger.Info($"Initialization has been triggered.");
            };

            _registry.OnInfrastructureDestroyed += (entry) =>
            {
                using var scope = _logger.CreateScope(entry.InfrastructureType.Name);
                _logger.Info($"Destroy has been triggered.");
            };
        }

        private IInfrastructureCleaner? RetrieveCleaner(InfrastuctureRegistryEntry entry)
        {
            _logger.Info("Cleanup in progress...");


            var assemblyName = new AssemblyName(entry.InfrastructureType.Assembly.FullName!);

            var targetAssembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName.Name);

            Type? infrastructureType = targetAssembly?.GetType(entry.InfrastructureType.FullName!);
            CleanerAttribute? attr = infrastructureType?.GetCustomAttribute<CleanerAttribute>(inherit: true);

            if (attr != null)
            {
                _logger.Info($"Cleaning using {attr.CleanerType.Name}.");
                return Activator.CreateInstance(attr.CleanerType) as IInfrastructureCleaner;
            }

            _logger.Error("No [Cleaner] attribute found. Skipping.");
            return null;
        }

        private async Task WaitForTestProcess(int pid)
        {
            Process? process = null;
            try
            {
                process = Process.GetProcessById(pid);
            }
            catch (ArgumentException)
            {
                _logger.Error($"No process with PID {pid} found. Exiting.");
                Console.Read();
                Environment.Exit(0);
            }

            _logger.Info($"Monitoring PID {pid} - awaiting termination...");
            await process!.WaitForExitAsync();
        }
    }
}

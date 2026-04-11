using Microsoft.Data.Sqlite;

using NotoriousTest.Core;
using NotoriousTest.Core.Infrastructures.Cleaner;
using NotoriousTest.Core.Registry;
using NotoriousTest.DoggyDog;
using NotoriousTest.SqlLiteRegistry;
using NotoriousTest.Watchdog;

using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;

try
{
    Arguments arguments = Arguments.From(args);
    var version = Assembly
        .GetExecutingAssembly().GetName().Version!;

    Banner.Print($"{version.Major}.{version.Minor}.{version.Build}", arguments.Pid, arguments.EnvironmentId);
    Console.WriteLine(string.Join(" ", arguments.runtimesPath));
    var resolver = new AssemblyDependencyResolver(arguments.AssemblyPath);
    AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
    {
        var assemblyName = new AssemblyName(args.Name);
        Logger.DarkGray(() => Console.WriteLine($"[DoggyDog] Resolving {args.Name} in test assembly"));
        string? assemblyPathFromTestAssembly = resolver.ResolveAssemblyToPath(assemblyName);

        if (assemblyPathFromTestAssembly != null && File.Exists(assemblyPathFromTestAssembly))
        {
            Logger.Gray(() => Console.WriteLine($"[DoggyDog] Assembly {assemblyName.ToString()} found in test assembly"));
            return Assembly.LoadFrom(assemblyPathFromTestAssembly);
        }
        Logger.DarkGray(() => Console.WriteLine($"[DoggyDog] Assembly {args.Name} not found in test assembly."));

        if ((assemblyName.Name!.StartsWith("Microsoft") || assemblyName.Name.StartsWith("System")) && arguments.runtimesPath.Length > 0)
        {
            foreach (var runtimePath in arguments.runtimesPath)
            {
                var parts = runtimePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var version = parts[^1];
                var name = parts[^2];

                Logger.DarkGray(() => Console.WriteLine($"[DoggyDog] Resolving {args.Name} from test assembly runtime {name} v{version}"));

                var candidate = Path.Combine(runtimePath, assemblyName.Name + ".dll");
                if (File.Exists(candidate))
                {
                    Logger.Gray(() => Console.WriteLine($"[DoggyDog] Assembly {assemblyName.ToString()} found in {name} v{version}"));
                    return Assembly.LoadFrom(candidate);
                }
            }
        }
        Logger.Gray(() => Console.WriteLine($"[DoggyDog] Unable to load {args.Name} assembly."));

        return null;
    };

    int processId = arguments.Pid;
    Assembly testAssembly = Assembly.LoadFrom(arguments.AssemblyPath);
    Logger.Cyan(() => Console.WriteLine($"[DoggyDog] Attached to test process with PID {processId} and EID {arguments.EnvironmentId}"));

    var cs = new SqliteConnectionStringBuilder(arguments.ConnectionString); // Validate connection string format early

    if (!File.Exists(cs.DataSource))
    {
        Logger.Red(() =>
        {
            Console.WriteLine($"[DoggyDog] Registry file not found at {cs.DataSource}");
            Environment.Exit(1);
        });
    }
    else
    {
        Logger.Yellow(() =>
        {
            Console.WriteLine($"[DoggyDog] Using registry file at {cs.DataSource}");
        });
    }

    IRegistry registry = new SqliteRegistryProvider(new SqliteRegistryProviderConfiguration()
    {
        ConnectionString = arguments.ConnectionString
    });

    Process? process = null;
    try
    {
        process = Process.GetProcessById(processId);
    }
    catch (ArgumentException)
    {
        Logger.Red(() => Console.WriteLine($"[DoggyDog] No process with PID {processId} found. Exiting."));
        Console.ReadLine();
        Environment.Exit(0);
    }

    Logger.DarkGray(() => Console.WriteLine($"[DoggyDog] Monitoring PID {processId} - awaiting termination..."));
    await process.WaitForExitAsync();



    if (DoggyDogWatchDog.ReadSuccessSignal(arguments.EnvironmentId))
    {
        Logger.Green(() => Console.WriteLine($"[DoggyDog] Process {processId} for EID {arguments.EnvironmentId} exited cleanly. No recovery needed."));
        Logger.Gray(() => Console.WriteLine($"[DoggyDog] Success signal found for {arguments.EnvironmentId}."));
        Environment.Exit(0);
    }

    Logger.Red(() => Console.WriteLine($"[DoggyDog] Process {processId} for EID {arguments.EnvironmentId} exited abnormally. Initiating crash recovery..."));

    IReadOnlyList<InfrastuctureRegistryEntry> entries = (await registry.GetByEnvironmentId(new EnvironmentId(arguments.EnvironmentId))).ToList();

    if (entries.Count == 0)
    {
        Logger.Yellow(() => Console.WriteLine($"[DoggyDog] No registered infrastructure found for EID {arguments.EnvironmentId}. Nothing to clean up."));
        Console.ReadLine();
        Environment.Exit(0);
    }

    Logger.Cyan(() => Console.WriteLine($"[DoggyDog] {entries.Count} infrastructure(s) registered for EID {arguments.EnvironmentId}. Starting cleanup..."));


    foreach (var entry in entries)
    {
        Logger.Yellow(() => Console.WriteLine($"  > [{entry.InfrastructureType.Name}] Cleanup in progress..."));

        var assemblyName = new AssemblyName(entry.InfrastructureType.AssemblyQualifiedName
            .Split(',')
            .Skip(1)
            .First()
            .Trim());

        var targetAssembly = AppDomain.CurrentDomain
            .GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == assemblyName.Name);

        Type? infrastructureType = targetAssembly?.GetType(entry.InfrastructureType.FullName!);
        CleanerAttribute? attr = infrastructureType?.GetCustomAttribute<CleanerAttribute>(inherit: true);

        if (attr != null)
        {
            Logger.DarkGray(() => Console.WriteLine($"  > [{entry.InfrastructureType.Name}] Cleaning using {attr.CleanerType.Name}."));

            IInfrastructureCleaner? cleaner = Activator.CreateInstance(attr.CleanerType) as IInfrastructureCleaner;

            if (cleaner != null)
            {
                await cleaner.CleanAfterCrash(entry.EnvironmentId, entry.InfrastructureId, entry.Metadata);
                Logger.Green(() => Console.WriteLine($"  > [{entry.InfrastructureType.Name}] Cleanup successful. Removing registry entry..."));
                await registry.Remove(entry.InfrastructureId);
                Logger.DarkGray(() => Console.WriteLine($"  > [{entry.InfrastructureType.Name}] Registry entry removed."));
            }
            else
            {
                Logger.Red(() => Console.WriteLine($"  > [{entry.InfrastructureType.Name}] Failed to instantiate cleaner. Skipping."));
                continue;
            }
        }
        else
        {
            Logger.Red(() => Console.WriteLine($"  > [{entry.InfrastructureType.Name}] No [Cleaner] attribute found. Skipping."));
            continue;
        }
    }

    Logger.Green(() => Console.WriteLine($"[DoggyDog] Crash recovery complete for PID {processId} and EID {arguments.EnvironmentId}."));
    Console.Read();
    Environment.Exit(0);
}
catch (Exception ex)
{
    Logger.Red(() =>
    {
        Console.WriteLine($"[DoggyDog] Fatal error during execution.");
        Console.WriteLine(ex.ToString());
        Console.WriteLine("Press any key to exit...");
    });
    Console.Read();
    Environment.Exit(1);
}
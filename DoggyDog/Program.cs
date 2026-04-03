using Microsoft.Data.Sqlite;

using NotoriousTest.Core.Infrastructures.Cleaner;
using NotoriousTest.Core.Registry;
using NotoriousTest.DoggyDog;
using NotoriousTest.SqlLiteRegistry;

using System.Diagnostics;
using System.Reflection;

try
{
    Arguments arguments = Arguments.From(args);
    var version = Assembly
        .GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
        ?.InformationalVersion ?? "?";

    Banner.Print(version, arguments.Pid);

    AppDomain.CurrentDomain.AssemblyResolve += (sender, resolveArgs) =>
    {
        var assemblyName = new AssemblyName(resolveArgs.Name);
        var name = assemblyName.Name + ".dll";
        var baseDir = Path.GetDirectoryName(arguments.AssemblyPath)!;

        var testPath = Path.Combine(baseDir, name);
        if (File.Exists(testPath)) return Assembly.LoadFrom(testPath);

        if (!string.IsNullOrEmpty(assemblyName.CultureName))
        {
            var culturePath = Path.Combine(baseDir, assemblyName.CultureName, name);
            if (File.Exists(culturePath)) return Assembly.LoadFrom(culturePath);
        }

        return null;
    };

    int processId = arguments.Pid;
    Assembly testAssembly = Assembly.LoadFrom(arguments.AssemblyPath);
    Logger.Cyan(() => Console.WriteLine($"[DoggyDog] Attached to test process (PID {processId})"));

    var cs = new SqliteConnectionStringBuilder(arguments.ConnectionString); // Validate connection string format early

    if (!File.Exists(cs.DataSource))
    {
        Logger.Red(() =>
        {
            Console.WriteLine($"[DoggyDog] Registry file not found at {cs.DataSource}");
            Environment.Exit(-1);
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

    Logger.DarkGray(() => Console.WriteLine($"[DoggyDog] Monitoring PID {processId} — awaiting termination..."));
    await process.WaitForExitAsync();

    if (process.ExitCode == 0)
    {
        Logger.Green(() => Console.WriteLine($"[DoggyDog] Process {processId} exited cleanly (code {process.ExitCode}). No recovery needed."));
        Environment.Exit(0);
    }

    Logger.Red(() => Console.WriteLine($"[DoggyDog] Process {processId} exited with code {process.ExitCode}. Initiating crash recovery..."));

    IReadOnlyList<InfrastuctureRegistryEntry> entries = (await registry.GetByProcessId(processId)).ToList();

    if (entries.Count == 0)
    {
        Logger.Yellow(() => Console.WriteLine($"[DoggyDog] No registered infrastructure found for PID {processId}. Nothing to clean up."));
        Console.ReadLine();
        Environment.Exit(0);
    }

    Logger.Cyan(() => Console.WriteLine($"[DoggyDog] {entries.Count} infrastructure(s) registered for PID {processId}. Starting cleanup..."));

    foreach (var entry in entries)
    {
        Logger.Yellow(() => Console.WriteLine($"  > [{entry.InfrastructureType.Name}] Cleanup in progress..."));

        Type? infrastructureType = testAssembly.GetLoadableTypes().FirstOrDefault(t => t.AssemblyQualifiedName == entry.InfrastructureType.AssemblyQualifiedName);
        CleanerAttribute? attr = infrastructureType?.GetCustomAttribute<CleanerAttribute>();

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

    Logger.Green(() => Console.WriteLine($"[DoggyDog] Crash recovery complete for PID {processId}."));
    Console.ReadLine();
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
    Console.ReadKey();
    Environment.Exit(1);
}
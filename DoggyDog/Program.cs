using DoggyDog;

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
using System.Text.Json;

try
{
    Arguments arguments = Arguments.From(args);
    var version = Assembly
        .GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
        ?.InformationalVersion ?? "?";

    Banner.Print(version, arguments.Pid, arguments.EnvironmentId);

    var testAssemblyDir = Path.GetDirectoryName(arguments.AssemblyPath)!;
    var assemblyLoader = new DoggyDogAssemblyLoadContext(arguments.AssemblyPath);
    Assembly testAssembly = assemblyLoader.LoadFromAssemblyPath(arguments.AssemblyPath);

    AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
    {
        var name = new AssemblyName(args.Name);
        var resolver = new AssemblyDependencyResolver(arguments.AssemblyPath);

        var path = resolver.ResolveAssemblyToPath(name);

        if (path == null)
        {
            var trustedAssemblies = (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string)
                ?.Split(Path.PathSeparator);


            path = trustedAssemblies?.FirstOrDefault(p =>
                Path.GetFileNameWithoutExtension(p).Equals(name.Name, StringComparison.OrdinalIgnoreCase));
        }

        return path != null ? assemblyLoader.LoadFromAssemblyPath(path) : null;
    };

    int processId = arguments.Pid;

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

        Type? infrastructureType = testAssembly.GetLoadableTypes().FirstOrDefault(t => t.AssemblyQualifiedName == entry.InfrastructureType.AssemblyQualifiedName);
        Attribute? attr = infrastructureType?.GetCustomAttributes().FirstOrDefault(attr => attr.GetType().Name == typeof(CleanerAttribute).Name);

        if (attr != null)
        {
            PropertyInfo info = attr.GetType().GetProperty(nameof(CleanerAttribute.CleanerType));
            Type cleanerType = (Type)info.GetValue(attr);

            Logger.DarkGray(() => Console.WriteLine($"  > [{entry.InfrastructureType.Name}] Cleaning using {cleanerType.Name}."));

            object? cleaner = Activator.CreateInstance(cleanerType);

            if (cleaner != null)
            {
                var coreAssemblyName = testAssembly.GetReferencedAssemblies()
                    .First(a => a.Name == typeof(EnvironmentId).Assembly.GetName().Name);

                var coreAssembly = assemblyLoader.LoadFromAssemblyName(coreAssemblyName);
                var envType = coreAssembly.GetType(typeof(EnvironmentId).FullName);
                ConstructorInfo ctorInfo = envType.GetConstructor([typeof(Guid)]);
                object envId = ctorInfo.Invoke([entry.EnvironmentId]);

                MethodInfo method = cleaner.GetType().GetMethod(nameof(IInfrastructureCleaner<>.CleanAfterCrash));

                var metadataType = method.GetParameters()[2].ParameterType;
                var json = JsonSerializer.Serialize(entry.Metadata);
                var metadata = JsonSerializer.Deserialize(json, metadataType);
                await (method.Invoke(cleaner, [envId, entry.InfrastructureId, metadata]) as Task);

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
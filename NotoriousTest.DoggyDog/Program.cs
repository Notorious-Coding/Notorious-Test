using NotoriousTest.Core.Infrastructures.Cleaner;
using NotoriousTest.Core.Registry;
using NotoriousTest.DoggyDog;
using NotoriousTest.SqlLiteRegistry;

using System.Diagnostics;
using System.Reflection;



try
{



    Arguments arguments = Arguments.From(args);

    AppDomain.CurrentDomain.AssemblyResolve += (sender, resolveArgs) =>
    {
        var assemblyName = new AssemblyName(resolveArgs.Name);
        var name = assemblyName.Name + ".dll";
        var baseDir = Path.GetDirectoryName(arguments.AssemblyPath)!;

        // Cherche dans le bin
        var testPath = Path.Combine(baseDir, name);
        if (File.Exists(testPath)) return Assembly.LoadFrom(testPath);

        // Cherche dans le sous-dossier culture
        if (!string.IsNullOrEmpty(assemblyName.CultureName))
        {
            var culturePath = Path.Combine(baseDir, assemblyName.CultureName, name);
            if (File.Exists(culturePath)) return Assembly.LoadFrom(culturePath);
        }

        return null;
    };

    var assemblyDir = Path.GetDirectoryName(arguments.AssemblyPath)!;
    int processId = arguments.Pid;
    Assembly testAssembly = Assembly.LoadFrom(arguments.AssemblyPath);


    Console.WriteLine($"Starting DoggyDog for test process pid: {processId}");
    IRegistry registry = new SqliteRegistryProvider();

    Process process = Process.GetProcessById(processId);
    Console.WriteLine($"Waiting for test process {processId} to exit");
    await process.WaitForExitAsync();

    if (process.ExitCode == 0)
    {
        Console.WriteLine($"Test process {processId} has terminated correctly, no crash recovery needed. ExitCode: {process.ExitCode}");
        Console.ReadLine();
        Environment.Exit(0);
    }

    Console.WriteLine($"Test process has terminated incorreclty, starting infrastructure clean up. Exit Code: {process.ExitCode}");

    // Récupérer toutes les infrastructures existante lié au PID passé en paramètre
    IReadOnlyList<InfrastuctureRegistryEntry> entries = (await registry.GetByProcessId(processId)).ToList();
    if (entries.Count == 0)
    {
        Console.WriteLine("No infrastructure found. Exiting.");
        Console.ReadLine();
        Environment.Exit(0);
    }

    Console.WriteLine($"Found {entries.Count()} infrastructures associated with test process {processId}");
    // Récuperer le type des metadata
    foreach (var entry in entries)
    {
        Console.WriteLine($"Cleaning up {entry.InfrastructureType.Name} ...");
        Type? infrastuctureType = testAssembly.GetLoadableTypes().FirstOrDefault(t => t.AssemblyQualifiedName == entry.InfrastructureType.AssemblyQualifiedName);
        CleanerAttribute? attr = infrastuctureType?.GetCustomAttribute<CleanerAttribute>();

        if (attr != null)
        {
            IInfrastructureCleaner? cleaner = Activator.CreateInstance(attr.CleanerType) as IInfrastructureCleaner;

            if (cleaner != null)
            {
                await cleaner.CleanAfterCrash(entry.EnvironmentId, entry.InfrastructureId, entry.Metadata);
                Console.WriteLine($"Clean up for {entry.InfrastructureType.Name} has been a success. Deleting from registry...");
                await registry.Remove(entry.InfrastructureId);
                Console.WriteLine("Registry entry has been deleted.");
            }
            else
            {
                Console.WriteLine($"No cleaner found for {entry.InfrastructureType.Name}. Cleanup aborted.");
            }
        }
        else
        {
            Console.WriteLine($"No cleaner attributres found for {entry.InfrastructureType.Name}. Cleanup aborted.");
        }
    }

    Console.WriteLine($"End of cleaning for test process {processId}. Exiting...");
    Console.ReadLine();
    Environment.Exit(0);
}
catch (Exception ex)
{
    Console.WriteLine($"Fatal error: {ex}");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
    Environment.Exit(1);
}

public static class AssemblyExtensions
{
    public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null)!;
        }
    }
}
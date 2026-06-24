using System.Reflection;
using System.Runtime.Loader;
using DoggyDog.Watchdog.Logs;

namespace DoggyDog.Watchdog.AssemblyLoader;

public class TestAssemblyLoader : ITestAssemblyLoader
{
    private readonly string _assemblyPath;
    private readonly string[] _runtimesPath;
    private readonly AssemblyDependencyResolver _resolver;

    public TestAssemblyLoader(string assemblyPath, string[] runtimesPath)
    {
        _resolver = new AssemblyDependencyResolver(assemblyPath);
        _assemblyPath = assemblyPath;
        _runtimesPath = runtimesPath;
    }

    protected Logger Log { get; } = Logger.Instance;

    public Assembly Load()
    {
        AppDomain.CurrentDomain.AssemblyResolve += ResolveAssemblies;
        var testAssembly = Assembly.LoadFrom(_assemblyPath);
        return testAssembly;
    }

    private Assembly? ResolveAssemblies(object? sender, ResolveEventArgs? args)
    {
        using LogScope scope = Log.CreateScope("AssemblyResolve", "Resolving test assembly types");

        AssemblyName assemblyName = new(args.Name);
        Assembly? assembly = ResolveFromTestAssembly(assemblyName);

        if (assembly != null) return assembly;

        if ((assemblyName.Name!.StartsWith("Microsoft") || assemblyName.Name.StartsWith("System")) &&
            _runtimesPath.Length > 0)
            foreach (string runtimePath in _runtimesPath)
            {
                assembly = ResolveFromSharedFramework(runtimePath, assemblyName);
                if (assembly != null) return assembly;
            }

        Log.Debug($"Unable to load {args.Name} assembly.");
        return null;
    }

    private Assembly? ResolveFromTestAssembly(AssemblyName assemblyName)
    {
        using LogScope scope = Log.CreateScope("TestAssembly");
        Log.Debug($"Resolving {assemblyName} in test assembly");
        string? assemblyPathFromTestAssembly = _resolver.ResolveAssemblyToPath(assemblyName);

        if (assemblyPathFromTestAssembly != null && File.Exists(assemblyPathFromTestAssembly))
        {
            Log.Debug($"Assembly {assemblyName} found in test assembly");
            return Assembly.LoadFrom(assemblyPathFromTestAssembly);
        }

        Log.Debug($"Assembly {assemblyName} not found in test assembly.");
        return null;
    }

    private Assembly? ResolveFromSharedFramework(string runtime, AssemblyName assemblyName)
    {
        using LogScope scope = Log.CreateScope("SharedFramework");
        string[] parts = runtime.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string version = parts[^1];
        string name = parts[^2];

        Log.Debug($"Resolving {assemblyName} from test assembly runtime {name} v{version}");

        string candidate = Path.Combine(runtime, assemblyName.Name + ".dll");
        if (File.Exists(candidate))
        {
            Log.Debug($"Assembly {assemblyName} found in {name} v{version}");
            return Assembly.LoadFrom(candidate);
        }

        return null;
    }
}

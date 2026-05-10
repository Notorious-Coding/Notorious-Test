using DoggyDog.Logs;

using System.Reflection;
using System.Runtime.Loader;

namespace DoggyDog.AssemblyLoader
{
    public class TestAssemblyLoader : ITestAssemblyLoader
    {
        private readonly string _assemblyPath;
        private readonly string[] _runtimesPath;
        private AssemblyDependencyResolver _resolver;
        protected Logger Log { get; } = Logger.Instance;
        public TestAssemblyLoader(string assemblyPath, string[] runtimesPath)
        {
            _resolver = new AssemblyDependencyResolver(assemblyPath);
            _assemblyPath = assemblyPath;
            _runtimesPath = runtimesPath;
        }

        public Assembly Load()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssemblies;
            Assembly testAssembly = Assembly.LoadFrom(_assemblyPath);
            return testAssembly;
        }

        private Assembly? ResolveAssemblies(object? sender, ResolveEventArgs? args)
        {
            using var scope = Log.CreateScope("AssemblyResolve", "Resolving test assembly types");

            AssemblyName assemblyName = new(args.Name);
            Assembly? assembly = ResolveFromTestAssembly(assemblyName);

            if (assembly != null) return assembly;

            if ((assemblyName.Name!.StartsWith("Microsoft") || assemblyName.Name.StartsWith("System")) && _runtimesPath.Length > 0)
            {
                foreach (var runtimePath in _runtimesPath)
                {
                    assembly = ResolveFromSharedFramework(runtimePath, assemblyName);
                    if (assembly != null) return assembly;
                }
            }
            Log.Debug($"Unable to load {args.Name} assembly.");
            return null;
        }

        private Assembly? ResolveFromTestAssembly(AssemblyName assemblyName)
        {
            using var scope = Log.CreateScope("TestAssembly");
            Log.Debug($"Resolving {assemblyName.ToString()} in test assembly");
            string? assemblyPathFromTestAssembly = _resolver.ResolveAssemblyToPath(assemblyName);

            if (assemblyPathFromTestAssembly != null && File.Exists(assemblyPathFromTestAssembly))
            {
                Log.Debug($"Assembly {assemblyName.ToString()} found in test assembly");
                return Assembly.LoadFrom(assemblyPathFromTestAssembly);
            }
            Log.Debug($"Assembly {assemblyName.ToString()} not found in test assembly.");
            return null;
        }

        private Assembly? ResolveFromSharedFramework(string runtime, AssemblyName assemblyName)
        {
            using var scope = Log.CreateScope("SharedFramework");
            var parts = runtime.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var version = parts[^1];
            var name = parts[^2];

            Log.Debug($"Resolving {assemblyName.ToString()} from test assembly runtime {name} v{version}");

            var candidate = Path.Combine(runtime, assemblyName.Name + ".dll");
            if (File.Exists(candidate))
            {
                Log.Debug($"Assembly {assemblyName.ToString()} found in {name} v{version}");
                return Assembly.LoadFrom(candidate);
            }

            return null;
        }
    }
}

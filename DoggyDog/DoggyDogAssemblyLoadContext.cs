using System.Reflection;
using System.Runtime.Loader;

namespace DoggyDog
{
    public class DoggyDogAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly string _basePath;

        public DoggyDogAssemblyLoadContext(string assemblyPath)
            : base("DoggyDog.TestContext", isCollectible: false)
        {
            _basePath = Path.GetDirectoryName(assemblyPath)!;
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var fileName = assemblyName.Name + ".dll";

            var fullPath = Path.Combine(_basePath, fileName);
            if (File.Exists(fullPath))
                return LoadFromAssemblyPath(fullPath);

            if (!string.IsNullOrEmpty(assemblyName.CultureName))
            {
                var culturePath = Path.Combine(_basePath, assemblyName.CultureName, fileName);
                if (File.Exists(culturePath))
                    return LoadFromAssemblyPath(culturePath);
            }

            return null; // fallback vers le shared framework (runtime de DoggyDog)
        }
    }
}

using System.Reflection;

public static class AssemblyExtensions
{
    extension(Assembly assembly)
    {
        public IEnumerable<Type> GetLoadableTypes()
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
}
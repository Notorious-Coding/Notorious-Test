using System.Reflection;

namespace NotoriousTest.Core.Runtime
{
    public interface IRuntime
    {
        RuntimeConfiguration? GetSupportedRuntimes(Assembly assembly);
    }
}

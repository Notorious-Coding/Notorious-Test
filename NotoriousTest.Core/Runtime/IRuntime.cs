using System.Reflection;

namespace NotoriousTest.Core.Runtime
{
    public interface IRuntime
    {
        public RuntimeConfiguration GetSupportedRuntimes(Assembly assembly);
    }
}

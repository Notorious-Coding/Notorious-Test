using System.Diagnostics;
using System.Reflection;

namespace NotoriousTest.Core.Watchdog
{
    public interface IWatchDog
    {
        Process Start(Assembly currentAssembly, int currentPid);
    }
}

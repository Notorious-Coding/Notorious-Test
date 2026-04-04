using System.Diagnostics;
using System.Reflection;

namespace NotoriousTest.Core.Watchdog
{
    public interface IWatchDog
    {
        Process Start(Assembly currentAssembly, int currentPid, EnvironmentId environmentId);
        void SendSuccessSignal(EnvironmentId contextId);
    }
}

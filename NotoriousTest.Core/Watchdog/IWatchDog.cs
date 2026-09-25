using System.Diagnostics;
using System.Reflection;

namespace NotoriousTest.Core.Watchdog;

/// <summary>
///     Watchdog provider interface.
///     Responsible for interacting with the additional watchdog process.
/// </summary>
public interface IWatchDog
{
    /// <summary>
    ///     Start the watchdog process.
    /// </summary>
    /// <param name="currentAssembly">Assembly running the tests.</param>
    /// <param name="currentPid">Current process PID.</param>
    /// <param name="environmentId">Current environment identifier.</param>
    /// <param name="runtimePaths">.NET runtimes path referenced by the current assembly.</param>
    /// <returns>The watchdog process started.</returns>
    Process Start(Assembly currentAssembly, int currentPid, EnvironmentId environmentId,
        IEnumerable<string>? runtimePaths);

    /// <summary>
    ///     Send the watchdog process that the current test campaign has finished successfully.
    /// </summary>
    /// <param name="environmentId">Environment identifier.</param>
    void SendSuccessSignal(EnvironmentId environmentId);
}

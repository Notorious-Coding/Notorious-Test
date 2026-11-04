using System.Diagnostics;

namespace NotoriousTest.Processes;

/// <summary>
///     Metadata of an infrastructure running in a local process.
///     Used by <see cref="ProcessInfrastructureCleaner" /> to kill the process after a test crash.
/// </summary>
public class ProcessMetadata
{
    /// <summary>
    ///     PID of the root process started by the infrastructure.
    /// </summary>
    public int ProcessId { get; set; }

    /// <summary>
    ///     Start time (UTC) of the root process.
    ///     Lets the cleaner check that the PID has not been reused by another process since the crash.
    /// </summary>
    public DateTime StartTimeUtc { get; set; }

    /// <summary>
    ///     Creates the metadata of a running process.
    /// </summary>
    /// <param name="process">Process started by the infrastructure.</param>
    public static ProcessMetadata From(Process process) => new()
    {
        ProcessId = process.Id,
        StartTimeUtc = process.StartTime.ToUniversalTime()
    };
}

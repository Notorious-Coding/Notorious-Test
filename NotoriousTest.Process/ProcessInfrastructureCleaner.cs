using System.Diagnostics;
using NotoriousTest.Core;
using NotoriousTest.Core.Infrastructures.Cleaner;

namespace NotoriousTest.Processes;

/// <summary>
///     Kills the process (and its descendants) of an infrastructure after a test crash, using its PID.
/// </summary>
public class ProcessInfrastructureCleaner : IInfrastructureCleaner<ProcessMetadata>
{
    // Process start times read back from the OS are not always exactly the same (precision differs between APIs).
    private static readonly TimeSpan StartTimeTolerance = TimeSpan.FromSeconds(1);

    /// <inheritdoc />
    public async Task CleanAfterCrash(EnvironmentId contextId, Guid infrastructureId,
        ProcessMetadata? metadata = null)
    {
        if (metadata == null) return;

        using Process? process = FindProcess(metadata.ProcessId);
        if (process == null) return; // Already exited.

        if (!IsSameProcess(process, metadata)) return; // PID reused by another process: do not kill it.

        await ProcessTree.KillAsync(process);
    }

    private static Process? FindProcess(int processId)
    {
        try
        {
            return Process.GetProcessById(processId);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static bool IsSameProcess(Process process, ProcessMetadata metadata)
    {
        try
        {
            TimeSpan delta = process.StartTime.ToUniversalTime() - metadata.StartTimeUtc;
            return delta.Duration() <= StartTimeTolerance;
        }
        catch (InvalidOperationException)
        {
            return false; // Exited in the meantime.
        }
    }
}

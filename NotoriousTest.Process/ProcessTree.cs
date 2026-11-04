using System.Diagnostics;

namespace NotoriousTest.Processes;

/// <summary>
///     Helpers to kill a process with all the processes it started.
/// </summary>
public static class ProcessTree
{
    /// <summary>
    ///     Kills the process and all its descendants.
    /// </summary>
    /// <param name="process">Root process of the tree.</param>
    public static async Task KillAsync(Process process)
    {
        if (process.HasExited) return;

        if (!OperatingSystem.IsWindows())
        {
            process.Kill(true);
            return;
        }

        // On Windows, Process.Kill(true) can miss grand-children started through cmd (e.g. cmd > npm > cmd > node):
        // taskkill /T walks the whole tree.
        using var taskkill = Process.Start(new ProcessStartInfo
        {
            FileName = "taskkill",
            ArgumentList = { "/T", "/F", "/PID", process.Id.ToString() },
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        })!;
        await taskkill.WaitForExitAsync();
    }
}

using System.Diagnostics;
using System.Text;

namespace NotoriousTest.Dependencies.Azure.FunctionCoreTools;

/// <summary>
///     Result of a process run: its exit code and its merged standard output / error logs.
/// </summary>
public record ProcessResult(int ExitCode, string Logs)
{
    public bool Succeeded => ExitCode == 0;
}

/// <summary>
///     Runs a process to completion and captures its logs (stdout and stderr, interleaved in arrival order).
/// </summary>
public static class ProcessRunner
{
    /// <exception cref="System.ComponentModel.Win32Exception">The executable could not be found or started.</exception>
    public static async Task<ProcessResult> RunAsync(string fileName, params string[] arguments)
    {
        var startInfo = new ProcessStartInfo(fileName)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (string argument in arguments) startInfo.ArgumentList.Add(argument);

        using var process = new Process { StartInfo = startInfo };

        // Output and error events are raised on different threads.
        var logs = new StringBuilder();
        DataReceivedEventHandler appendLine = (_, e) =>
        {
            if (e.Data is null) return;
            lock (logs)
            {
                logs.AppendLine(e.Data);
            }
        };
        process.OutputDataReceived += appendLine;
        process.ErrorDataReceived += appendLine;

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // Also waits for the redirected streams to be fully read.
        await process.WaitForExitAsync();

        lock (logs)
        {
            return new ProcessResult(process.ExitCode, logs.ToString());
        }
    }
}

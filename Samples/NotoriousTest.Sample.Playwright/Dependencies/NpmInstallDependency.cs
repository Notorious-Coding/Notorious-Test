using System.Diagnostics;
using NotoriousTest.Core.Infrastructures.Dependencies;

namespace NotoriousTest.Sample.Playwright.Dependencies;

/// <summary>
///     Runs <c>npm install</c> when the node_modules of the npm project are missing.
/// </summary>
public class NpmInstallDependency(string projectDir) : IInfrastructureDependency
{
    public Task<bool> Exist() => Task.FromResult(Directory.Exists(Path.Combine(projectDir, "node_modules")));

    public async Task Install()
    {
        var psi = new ProcessStartInfo
        {
            WorkingDirectory = projectDir,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (OperatingSystem.IsWindows())
        {
            // npm is a batch script on Windows (npm.cmd).
            psi.FileName = "cmd.exe";
            psi.ArgumentList.Add("/c");
            psi.ArgumentList.Add("npm");
        }
        else
        {
            psi.FileName = "npm";
        }

        psi.ArgumentList.Add("install");

        using var process = Process.Start(psi)!;
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"npm install failed in {projectDir} (exit code {process.ExitCode}).");
    }

    // node_modules are kept for the next test campaigns.
    public Task Uninstall() => Task.CompletedTask;
}

using System.ComponentModel;
using System.Diagnostics;
using NotoriousTest.Core.Infrastructures.Dependencies;

namespace NotoriousTest.Requirements.Docker;

/// <summary>
///     Requires Docker to be installed and its daemon to be running.
///     Docker is not installed automatically: it needs elevated privileges, may require a reboot and a license (Docker
///     Desktop).
///     See https://docs.docker.com/get-docker/.
/// </summary>
public class DockerRequirement : IInfrastructureRequirement
{
    /// <inheritdoc />
    public async Task<bool> Exist()
    {
        // "docker info" fails if the CLI is missing, but also if the daemon is not running.
        var startInfo = new ProcessStartInfo("docker", "info")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = Process.Start(startInfo)!;
            Task<string> stdOut = process.StandardOutput.ReadToEndAsync();
            Task<string> stdErr = process.StandardError.ReadToEndAsync();
            await Task.WhenAll(stdOut, stdErr, process.WaitForExitAsync());

            return process.ExitCode == 0;
        }
        catch (Win32Exception)
        {
            // Docker CLI not found.
            return false;
        }
    }

    // Every instance represents the same requirement, allowing the environment to check it only once.
    public override bool Equals(object? obj) => obj is DockerRequirement;

    public override int GetHashCode() => typeof(DockerRequirement).GetHashCode();
}

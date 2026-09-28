using System.ComponentModel;
using NotoriousTest.Core.Infrastructures.Dependencies;

namespace NotoriousTest.Dependencies.Azure.FunctionCoreTools;

/// <summary>
///     Azure Functions Core Tools (func CLI).
///     Installed with winget on Windows, and with apt-get from the Microsoft package repository on Debian/Ubuntu.
///     Both require elevated privileges (UAC prompt on Windows, root or passwordless sudo on Linux).
/// </summary>
public class AzureFunctionCoreToolsDependency : IInfrastructureDependency
{
    private const string WINGET_ID = "Microsoft.Azure.FunctionsCoreTools";
    private const string APT_PACKAGE = "azure-functions-core-tools-4";

    public async Task Install()
    {
        if (await Exist())
            return;
        ProcessResult result;
        if (OperatingSystem.IsWindows())
        {
            result = await ProcessRunner.RunAsync("winget",
                "install", "-e", "--silent", "--accept-package-agreements", "--accept-source-agreements",
                $"--id={WINGET_ID}");
        }
        else if (OperatingSystem.IsLinux())
        {
            string script = $"""
                             set -e
                             wget -q https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O /tmp/packages-microsoft-prod.deb
                             sudo dpkg -i /tmp/packages-microsoft-prod.deb
                             rm /tmp/packages-microsoft-prod.deb
                             sudo apt-get update
                             sudo DEBIAN_FRONTEND=noninteractive apt-get install -y {APT_PACKAGE}
                             """;

            result = await ProcessRunner.RunAsync("bash", "-c", script);
        }
        else
        {
            throw new PlatformNotSupportedException(
                "Azure Functions Core Tools can only be installed on Windows or Linux.");
        }

        if (!result.Succeeded)
            throw new Exception(
                $"Failed to install Azure Functions Core Tools (exit code {result.ExitCode}). Logs: {result.Logs}");
    }

    public async Task Uninstall()
    {
        if (!await Exist())
            return;
        ProcessResult result;
        if (OperatingSystem.IsWindows())
        {
            result = await ProcessRunner.RunAsync("winget",
                "uninstall", $"--id={WINGET_ID}", "-e", "--silent",
                "--disable-interactivity", "--accept-source-agreements");
        }
        else if (OperatingSystem.IsLinux())
        {
            string script = """
                            set -e
                            sudo -n env DEBIAN_FRONTEND=noninteractive apt-get remove -y azure-functions-core-tools-4
                            """;

            result = await ProcessRunner.RunAsync("bash", "-c", script);
        }
        else
        {
            throw new PlatformNotSupportedException(
                "Azure Functions Core Tools can only be uninstalled on Windows or Linux.");
        }

        if (!result.Succeeded)
            throw new Exception(
                $"Failed to uninstall Azure Functions Core Tools (exit code {result.ExitCode}). Logs: {result.Logs}");
    }

    public async Task<bool> Exist()
    {
        try
        {
            ProcessResult result = await ProcessRunner.RunAsync("func", "--version");
            return result.Succeeded;
        }
        catch (Win32Exception)
        {
            // func CLI not found.
            return false;
        }
    }
}

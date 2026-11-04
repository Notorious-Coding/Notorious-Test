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

    // winget exit codes meaning the package is already there: not an installation failure.
    private static readonly int[] WINGET_ALREADY_INSTALLED_EXIT_CODES =
    [
        unchecked((int)0x8A15002B), // APPINSTALLER_CLI_ERROR_UPDATE_NOT_APPLICABLE: installed, no newer version.
        unchecked((int)0x8A150061) // APPINSTALLER_CLI_ERROR_PACKAGE_ALREADY_INSTALLED
    ];

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

            if (WINGET_ALREADY_INSTALLED_EXIT_CODES.Contains(result.ExitCode))
                result = result with { ExitCode = 0 };

            // The installer adds func to the system PATH, not to the PATH of this (already running) process.
            if (result.Succeeded) RefreshPathFromSystem();
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
        if (await FuncRuns()) return true;

        // func may be installed after this process (or its parent: IDE, terminal) started, so the PATH inherited
        // by this process does not contain it yet. The func process started by the infrastructure resolves func
        // from this PATH too, so refreshing it is enough.
        if (!OperatingSystem.IsWindows() || !RefreshPathFromSystem()) return false;

        return await FuncRuns();
    }

    private static async Task<bool> FuncRuns()
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

    /// <summary>
    ///     Adds to the PATH of the current process the entries of the machine and user PATH it does not have yet.
    /// </summary>
    /// <returns>True if at least one entry has been added.</returns>
    private static bool RefreshPathFromSystem()
    {
        string current = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        List<string> entries = current.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries).ToList();
        var known = new HashSet<string>(entries.Select(NormalizePathEntry), StringComparer.OrdinalIgnoreCase);

        string?[] systemPaths =
        [
            Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine),
            Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User)
        ];

        bool added = false;
        foreach (string entry in systemPaths
                     .SelectMany(p => (p ?? string.Empty).Split(Path.PathSeparator,
                         StringSplitOptions.RemoveEmptyEntries)))
        {
            if (!known.Add(NormalizePathEntry(entry))) continue;
            entries.Add(Environment.ExpandEnvironmentVariables(entry.Trim()));
            added = true;
        }

        // Process scope only (no EnvironmentVariableTarget): the machine and user PATH are only read, never written.
        // The change lives in memory until the test process exits, and is inherited by its child processes (func...).
        if (added) Environment.SetEnvironmentVariable("PATH", string.Join(Path.PathSeparator, entries));
        return added;
    }

    private static string NormalizePathEntry(string entry) =>
        Environment.ExpandEnvironmentVariables(entry.Trim()).TrimEnd('\\', '/');
}

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Web.Helpers;
using NotoriousTest.Web.Infrastructures;

namespace NotoriousTest.Web.Npm;

/// <summary>
///     Create a web application infrastructure started with an npm script (<c>npm run &lt;Command&gt;</c>).
///     The port to listen on is given to the application with the <c>PORT</c> environment variable.
/// </summary>
public abstract class NpmWebApplicationInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
    : WebApplicationInfrastructure(contextId, logger, registry)
{
    private const string PORT_ENVIRONMENT_VARIABLE = "PORT";

    /// <summary>
    ///     Directory containing the npm project (where the <c>package.json</c> is).
    /// </summary>
    protected abstract string ProjectDir { get; }

    /// <summary>
    ///     Name of the npm script to run, as declared in the <c>scripts</c> section of the <c>package.json</c>.
    ///     e.g. <c>start</c>, <c>dev</c>.
    /// </summary>
    protected abstract string Command { get; }

    /// <summary>
    ///     Path requested to know if the application is listening. Any HTTP response means the application is ready.
    /// </summary>
    protected virtual string HealthCheckPath => "/";

    /// <summary>
    ///     Maximum time to wait for the application to be listening.
    /// </summary>
    protected virtual TimeSpan StartupTimeout => TimeSpan.FromMinutes(2);

    /// <summary>
    ///     Logs of the application.
    /// </summary>
    protected StringBuilder Logs { get; } = new();

    /// <summary>
    ///     Current state of the application.
    /// </summary>
    protected NpmWebApplicationState State { get; private set; } = NpmWebApplicationState.None;

    /// <summary>
    ///     Current process running the application.
    /// </summary>
    protected Process? NpmProcess { get; private set; }

    /// <inheritdoc />
    public override async Task Initialize() => HttpClient = await Start();

    /// <inheritdoc />
    public override async Task Destroy()
    {
        if (NpmProcess == null) return;

        // npm spawns the script (node, vite...) as a child process.
        if (!NpmProcess.HasExited) await KillProcessTreeAsync(NpmProcess);
        await NpmProcess.WaitForExitAsync();
        NpmProcess.Dispose();
        NpmProcess = null;
    }


    /// <summary>
    ///     Gives the configuration to the application, before it is started.
    ///     The application reads its configuration its own way: add it to the environment variables,
    ///     write it in a <c>.env</c> or JSON file, etc.
    /// </summary>
    /// <param name="configuration">
    ///     Configuration consumed from the other infrastructures, then
    ///     <see cref="WebApplicationInfrastructure.AdditionalConfiguration" />
    ///     (which wins on duplicate keys). Keys are flattened in the appsettings format,
    ///     e.g. <c>ConnectionStrings:Default</c>, <c>Brokers:0</c>.
    /// </param>
    /// <param name="environmentVariables">Environment variables of the npm process, which can be modified.</param>
    protected abstract Task ConfigureApplication(Dictionary<string, string?> configuration,
        IDictionary<string, string?> environmentVariables);

    /// <summary>
    ///     Additional arguments given to the npm script (after <c>--</c>).
    ///     Override it when the application does not read the <c>PORT</c> environment variable,
    ///     e.g. <c>["--port", port.ToString()]</c> for Vite.
    /// </summary>
    /// <param name="port">Port the application must listen on.</param>
    protected virtual IEnumerable<string> GetScriptArguments(int port) => [];

    private async Task<HttpClient> Start()
    {
        int freePort = GetFreeHttpPortOnMachine();
        NpmProcess = await StartNpmProcess(freePort);
        HttpClient client = CreateClient(freePort);
        State = await WaitForApplicationAsync(client, NpmProcess, StartupTimeout);

        if (State == NpmWebApplicationState.Failed)
            throw new Exception($"npm application failed to start with 'npm run {Command}'. Logs: {Logs}");

        OutputConfiguration.WebAppUrl = client.BaseAddress!.ToString();

        return client;
    }

    private HttpClient CreateClient(int port) =>
        new() { BaseAddress = new Uri($"http://localhost:{port}") };

    private async Task<NpmWebApplicationState> WaitForApplicationAsync(HttpClient client, Process process,
        TimeSpan timeout)
    {
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed < timeout)
        {
            if (process.HasExited) return NpmWebApplicationState.Failed;

            try
            {
                using HttpResponseMessage _ = await client.GetAsync(HealthCheckPath);
                return NpmWebApplicationState.Running;
            }
            catch (HttpRequestException)
            {
                // Application is not listening yet.
            }

            await Task.Delay(100);
        }

        return NpmWebApplicationState.Failed;
    }

    private async Task<Process> StartNpmProcess(int port)
    {
        var psi = new ProcessStartInfo
        {
            WorkingDirectory = ProjectDir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        List<string> npmArguments = ["run", Command];
        string[] scriptArguments = GetScriptArguments(port).ToArray();
        if (scriptArguments.Length > 0) npmArguments.AddRange(["--", ..scriptArguments]);

        if (OperatingSystem.IsWindows())
        {
            // npm is a batch script on Windows (npm.cmd): cmd resolves it from the PATH like a terminal does.
            psi.FileName = "cmd.exe";
            psi.ArgumentList.Add("/c");
            psi.ArgumentList.Add("npm");
        }
        else
        {
            psi.FileName = "npm";
        }

        foreach (string argument in npmArguments) psi.ArgumentList.Add(argument);

        psi.Environment[PORT_ENVIRONMENT_VARIABLE] = port.ToString();

        Dictionary<string, string?> configuration = (ConsumedConfiguration ?? []).ToAppSettings();
        foreach ((string key, string value) in AdditionalConfiguration) configuration[key] = value;
        await ConfigureApplication(configuration, psi.Environment);

        var process = Process.Start(psi)!;
        process.OutputDataReceived += (sender, e) => Logs.AppendLine(e.Data);
        process.ErrorDataReceived += (sender, e) => Logs.AppendLine(e.Data);
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        State = NpmWebApplicationState.Starting;
        return process;
    }

    private static async Task KillProcessTreeAsync(Process process)
    {
        if (!OperatingSystem.IsWindows())
        {
            process.Kill(true);
            return;
        }

        // On Windows, Process.Kill(true) misses the node process started by npm (cmd > npm > cmd > node).
        // It keeps the redirected output open, so WaitForExitAsync would never complete.
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

    private int GetFreeHttpPortOnMachine()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}

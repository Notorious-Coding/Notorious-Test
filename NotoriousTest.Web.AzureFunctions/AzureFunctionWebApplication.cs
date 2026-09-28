using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using NotoriousTest.Core.Configuration;
using NotoriousTest.Core.Infrastructures.Dependencies;
using NotoriousTest.Dependencies.Azure.FunctionCoreTools;
using NotoriousTest.Web.Applications;
using NotoriousTest.Web.Helpers;

namespace NotoriousTest.Web.AzureFunctions;

/// <summary>
///     Create an Azure Function application.
/// </summary>
public abstract class AzureFunctionWebApplication : IWebApplication
{
    /// <summary>
    ///     Directory containing the Azure Function project.
    /// </summary>
    public abstract string FunctionProjectDir { get; }

    /// <summary>
    ///     Additional configuration to be passed to the Azure Function.
    /// </summary>
    public virtual Dictionary<string, string> AdditionalConfiguration { get; set; } = new();

    /// <summary>
    ///     Logs of the Azure Function.
    /// </summary>
    protected StringBuilder Logs { get; } = new();


    /// <summary>
    ///     Current state of the Azure Function.
    /// </summary>
    protected AzureFunctionState AzureFunctionState { get; private set; } = AzureFunctionState.None;

    /// <summary>
    ///     Current process running the Azure Function.
    /// </summary>
    protected Process? AzureFunctionProcess { get; private set; }

    /// <summary>
    ///     Maximum time to wait for the Azure Function host to be running.
    ///     Includes the build of the function project done by <c>func host start</c>.
    /// </summary>
    protected virtual TimeSpan StartupTimeout => TimeSpan.FromMinutes(2);

    /// <summary>
    ///     Configuration consumed by the Azure Function from other infrastructures.
    /// </summary>
    public List<ConfigurationEntry<object>> ConsumedConfiguration { get; set; } = [];

    public async ValueTask DisposeAsync()
    {
        if (AzureFunctionProcess == null) return;

        // func host start spawns the dotnet worker as a child process.
        if (!AzureFunctionProcess.HasExited) AzureFunctionProcess.Kill(true);
        await AzureFunctionProcess.WaitForExitAsync();
        AzureFunctionProcess.Dispose();
        AzureFunctionProcess = null;
    }

    public List<IInfrastructureDependency> Dependencies { get; } = [new AzureFunctionCoreToolsDependency()];
    public List<IInfrastructureRequirement> Requirements { get; } = [];

    public async Task<HttpClient> Start()
    {
        int freePort = GetFreeHttpPortOnMachine();
        AzureFunctionProcess = StartAzureFunctionProcess(freePort);
        HttpClient client = CreateClient(freePort);
        AzureFunctionState = await WaitForHostStateAsync(client, AzureFunctionProcess, StartupTimeout);

        if (AzureFunctionState == AzureFunctionState.Failed)
            throw new Exception($"Azure Function failed to start. Logs: {Logs}");
        return client;
    }

    private HttpClient CreateClient(int port) =>
        new() { BaseAddress = new Uri($"http://localhost:{port}") };

    private async Task<AzureFunctionState> WaitForHostStateAsync(HttpClient client, Process process, TimeSpan timeout)
    {
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed < timeout)
        {
            if (process.HasExited) return AzureFunctionState.Failed;

            try
            {
                string json = await client.GetStringAsync("/admin/host/status");
                using var doc = JsonDocument.Parse(json);
                string? stateStr = doc.RootElement.GetProperty("state").GetString();
                if (stateStr == "Running") return AzureFunctionState.Running;
            }
            catch (HttpRequestException)
            {
                // Host is not ready yet.
            }

            await Task.Delay(100);
        }

        return AzureFunctionState.Failed;
    }

    private Process StartAzureFunctionProcess(int port)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "func",
            ArgumentList = { "host", "start", "--port", port.ToString() },
            WorkingDirectory = FunctionProjectDir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        psi.Environment["FUNCTIONS_WORKER_RUNTIME"] = "dotnet-isolated";
        Dictionary<string, string?> configuration = (ConsumedConfiguration ?? []).ToEnvironmentVariables();
        foreach ((string key, string? value) in configuration) psi.Environment[key] = value;

        foreach ((string key, string value) in AdditionalConfiguration) psi.Environment[key] = value;

        var process = Process.Start(psi)!;
        process.OutputDataReceived += (sender, e) => Logs.AppendLine(e.Data);
        process.ErrorDataReceived += (sender, e) => Logs.AppendLine(e.Data);
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        AzureFunctionState = AzureFunctionState.Starting;
        return process;
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

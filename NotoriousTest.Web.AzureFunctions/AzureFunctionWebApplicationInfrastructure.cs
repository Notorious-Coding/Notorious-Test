using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Dependencies.Azure.FunctionCoreTools;
using NotoriousTest.Web.Helpers;
using NotoriousTest.Web.Infrastructures;

namespace NotoriousTest.Web.AzureFunctions;

/// <summary>
///     Create an Azure Function application infrastructure.
/// </summary>
public abstract class AzureFunctionWebApplicationInfrastructure : WebApplicationInfrastructure
{
    /// <summary>
    ///     Represents the base class for creating Azure Function web application infrastructures.
    /// </summary>
    /// <remarks>
    ///     This class provides an abstract foundation for managing and initializing Azure Functions as part of a web
    ///     application infrastructure.
    ///     It extends the WebApplicationInfrastructure class and includes additional functionality specific to Azure
    ///     Functions.
    /// </remarks>
    public AzureFunctionWebApplicationInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry) :
        base(contextId, logger, registry)
    {
        Dependencies = [new AzureFunctionCoreToolsDependency()];
    }


    /// <summary>
    ///     Directory containing the Azure Function project.
    /// </summary>
    protected abstract string FunctionProjectDir { get; }

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

    /// <inheritdoc />
    public override async Task Destroy()
    {
        if (AzureFunctionProcess == null) return;

        // func host start spawns the dotnet worker as a child process.
        if (!AzureFunctionProcess.HasExited) AzureFunctionProcess.Kill(true);
        await AzureFunctionProcess.WaitForExitAsync();
        AzureFunctionProcess.Dispose();
        AzureFunctionProcess = null;
    }

    /// <inheritdoc />
    public override async Task Initialize() => HttpClient = await Start();

    private async Task<HttpClient> Start()
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

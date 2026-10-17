using System.Net;
using System.Net.Sockets;
using NotoriousTest.Core;
using NotoriousTest.Core.Configuration;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Web.AspNetCore;

namespace NotoriousTest.Sample.Playwright.Infrastructures;

/// <summary>
///     NotoriousTest.Sample.TestWebApp, started with its WebApplicationFactory.
///     Unlike the in-memory TestServer, the API listens on a real port with Kestrel,
///     so the front (and therefore the browser) can call it.
/// </summary>
public class ApiInfrastructure : AspNetCoreWebApplicationInfrastructure<KestrelWebApplication, Program>
{
    public const string API_URL_KEY = "ApiUrl";

    public ApiInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry) : base(contextId,
        logger, registry)
    {
    }

    // Started before the front, which needs the API url.
    public override int? Order => 998;

    public override async Task Initialize()
    {
        await base.Initialize();

        // Given to the next infrastructures (FrontInfrastructure).
        OutputConfiguration.Add(new ConfigurationEntry<object>(HttpClient!.BaseAddress!.ToString(), API_URL_KEY));
    }
}

/// <summary>
///     WebApplicationFactory listening on a free port with Kestrel.
/// </summary>
public class KestrelWebApplication : AspNetCoreWebApplication<Program>
{
    public override Task<HttpClient> Start()
    {
        int port = GetFreePort();

        UseKestrel(port);
        StartServer();

        return Task.FromResult(new HttpClient { BaseAddress = new Uri($"http://localhost:{port}") });
    }

    private static int GetFreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}

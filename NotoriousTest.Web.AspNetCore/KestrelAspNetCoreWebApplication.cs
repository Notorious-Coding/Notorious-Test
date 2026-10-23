using System.Net;
using System.Net.Sockets;

namespace NotoriousTest.Web.AspNetCore;

/// <summary>
///     Web application listening on a free local port with a real Kestrel server, instead of the in-memory TestServer.
///     Use it when something outside the test process (a browser, an npm application…) must call the application.
/// </summary>
/// <typeparam name="TEntryPoint">Type of the Program.cs file of the application to start.</typeparam>
public class KestrelAspNetCoreWebApplication<TEntryPoint> : AspNetCoreWebApplication<TEntryPoint>
    where TEntryPoint : class
{
    /// <summary>
    ///     Creates a web application listening on a free local port.
    ///     The <see cref="HttpClient" /> created by the factory targets the Kestrel server address.
    /// </summary>
    public KestrelAspNetCoreWebApplication()
    {
        UseKestrel(GetFreePort());
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

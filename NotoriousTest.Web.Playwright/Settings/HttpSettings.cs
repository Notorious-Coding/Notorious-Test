using Microsoft.Playwright;

namespace NotoriousTest.Web.Playwright.Settings;

/// <summary>
///     HTTP and network options of the browser context.
/// </summary>
public class HttpSettings
{
    /// <summary>
    ///     Additional HTTP headers sent with every request.
    /// </summary>
    public Dictionary<string, string>? ExtraHTTPHeaders { get; set; }

    /// <summary>
    ///     Credentials for HTTP authentication.
    /// </summary>
    public HttpCredentialsSettings? Credentials { get; set; }

    /// <summary>
    ///     Credentials for HTTP authentication, one per origin.
    /// </summary>
    public HttpCredentialsSettings[]? CredentialsList { get; set; }

    /// <summary>
    ///     Network proxy.
    /// </summary>
    public ProxySettings? Proxy { get; set; }

    /// <summary>
    ///     TLS client certificates.
    /// </summary>
    public ClientCertificateSettings[]? ClientCertificates { get; set; }

    /// <summary>
    ///     Whether to ignore HTTPS errors when sending network requests.
    /// </summary>
    public bool? IgnoreHTTPSErrors { get; set; }

    /// <summary>
    ///     Whether to emulate network being offline.
    /// </summary>
    public bool? Offline { get; set; }

    internal void ApplyTo(BrowserNewContextOptions options)
    {
        if (ExtraHTTPHeaders != null) options.ExtraHTTPHeaders = ExtraHTTPHeaders;
        if (Credentials != null) options.HttpCredentials = Credentials.ToHttpCredentials();
        if (CredentialsList != null)
            options.HttpCredentialsList = CredentialsList.Select(c => c.ToHttpCredentials()).ToList();
        if (Proxy != null) options.Proxy = Proxy.ToProxy();
        if (ClientCertificates != null)
            options.ClientCertificates = ClientCertificates.Select(c => c.ToClientCertificate()).ToList();
        if (IgnoreHTTPSErrors != null) options.IgnoreHTTPSErrors = IgnoreHTTPSErrors;
        if (Offline != null) options.Offline = Offline;
    }
}

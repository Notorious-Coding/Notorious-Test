using Microsoft.Playwright;

namespace NotoriousTest.Web.Playwright.Settings;

/// <summary>
///     Network proxy used by the browser context.
/// </summary>
public class ProxySettings
{
    /// <summary>
    ///     Proxy server, for example <c>http://myproxy.com:3128</c> or <c>socks5://myproxy.com:3128</c>.
    /// </summary>
    public string? Server { get; set; }

    /// <summary>
    ///     Comma-separated domains to bypass the proxy, for example <c>.com, chromium.org</c>.
    /// </summary>
    public string? Bypass { get; set; }

    /// <summary>
    ///     Username for the HTTP proxy.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    ///     Password for the HTTP proxy.
    /// </summary>
    public string? Password { get; set; }

    internal Proxy ToProxy() => new()
    {
        Server = Server!, Bypass = Bypass, Username = Username, Password = Password
    };
}

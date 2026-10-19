using Microsoft.Playwright;

namespace NotoriousTest.Web.Playwright.Settings;

/// <summary>
///     Permissions and security options of the browser context.
/// </summary>
public class SecuritySettings
{
    /// <summary>
    ///     Permissions granted to all pages, for example <c>geolocation</c> or <c>clipboard-read</c>.
    /// </summary>
    public string[]? Permissions { get; set; }

    /// <summary>
    ///     Whether to bypass the Content-Security-Policy of the pages.
    /// </summary>
    public bool? BypassCSP { get; set; }

    /// <summary>
    ///     Whether to enable JavaScript in the context.
    /// </summary>
    public bool? JavaScriptEnabled { get; set; }

    /// <summary>
    ///     Whether to automatically download all the attachments.
    /// </summary>
    public bool? AcceptDownloads { get; set; }

    /// <summary>
    ///     Whether to allow sites to register Service workers (Allow, Block).
    /// </summary>
    public ServiceWorkerPolicy? ServiceWorkers { get; set; }

    internal void ApplyTo(BrowserNewContextOptions options)
    {
        if (Permissions != null) options.Permissions = Permissions;
        if (BypassCSP != null) options.BypassCSP = BypassCSP;
        if (JavaScriptEnabled != null) options.JavaScriptEnabled = JavaScriptEnabled;
        if (AcceptDownloads != null) options.AcceptDownloads = AcceptDownloads;
        if (ServiceWorkers != null) options.ServiceWorkers = ServiceWorkers;
    }
}

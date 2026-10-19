using Microsoft.Playwright;
using NotoriousTest.Web.Playwright.Browsers;

namespace NotoriousTest.Web.Playwright.Settings;

/// <summary>
///     Browser settings, read from the <c>Browser</c> section of the testsettings.json file.
/// </summary>
public class BrowserSettings
{
    /// <summary>
    ///     Section of the testsettings.json file.
    /// </summary>
    public static string SECTION_KEY = "Browser";

    /// <summary>
    ///     Browser type (chromium, firefox, webkit). Only used by <see cref="SettingsBrowserInfrastructure" />.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    ///     Options used to launch the browser.
    /// </summary>
    public LaunchSettings? Launch { get; set; }

    /// <summary>
    ///     Emulated device.
    /// </summary>
    public DeviceSettings? Device { get; set; }

    /// <summary>
    ///     Locale, timezone and geolocation.
    /// </summary>
    public LocalizationSettings? Localization { get; set; }

    /// <summary>
    ///     Emulated media features.
    /// </summary>
    public AppearanceSettings? Appearance { get; set; }

    /// <summary>
    ///     HTTP and network options.
    /// </summary>
    public HttpSettings? Http { get; set; }

    /// <summary>
    ///     Permissions and security options.
    /// </summary>
    public SecuritySettings? Security { get; set; }

    /// <summary>
    ///     HAR and video recording.
    /// </summary>
    public RecordingSettings? Recording { get; set; }

    /// <summary>
    ///     Storage state the context is populated with.
    /// </summary>
    public StorageStateSettings? StorageState { get; set; }

    /// <summary>
    ///     Whether to enable the strict selectors mode.
    /// </summary>
    public bool? StrictSelectors { get; set; }

    /// <summary>
    ///     Build the Playwright launch options from these settings.
    /// </summary>
    /// <returns>Browser type launch options.</returns>
    public BrowserTypeLaunchOptions ToLaunchOptions() => Launch?.ToLaunchOptions() ?? new BrowserTypeLaunchOptions();

    /// <summary>
    ///     Build the Playwright context options from these settings.
    /// </summary>
    /// <param name="baseUrl">Base URL of the web application.</param>
    /// <returns>Browser new context options.</returns>
    public BrowserNewContextOptions ToContextOptions(string baseUrl)
    {
        var options = new BrowserNewContextOptions();

        Device?.ApplyTo(options);
        Localization?.ApplyTo(options);
        Appearance?.ApplyTo(options);
        Http?.ApplyTo(options);
        Security?.ApplyTo(options);
        Recording?.ApplyTo(options);
        StorageState?.ApplyTo(options);
        if (StrictSelectors != null) options.StrictSelectors = StrictSelectors;

        options.BaseURL = baseUrl;
        return options;
    }
}

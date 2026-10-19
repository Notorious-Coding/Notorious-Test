using Microsoft.Playwright;

namespace NotoriousTest.Web.Playwright.Settings;

/// <summary>
///     Options used to launch the browser, read from the <c>Browser:Launch</c> section.
/// </summary>
public class LaunchSettings
{
    /// <summary>
    ///     Whether to run the browser in headless mode. Set it to false to see the browser while debugging.
    /// </summary>
    public bool? Headless { get; set; }

    /// <summary>
    ///     Slows down Playwright operations by the specified amount of milliseconds.
    /// </summary>
    public float? SlowMo { get; set; }

    /// <summary>
    ///     Maximum time in milliseconds to wait for the browser to start. 0 disables the timeout.
    /// </summary>
    public float? Timeout { get; set; }

    /// <summary>
    ///     Browser distribution channel, for example <c>chrome</c>, <c>msedge</c> or <c>chrome-beta</c>.
    /// </summary>
    public string? Channel { get; set; }

    /// <summary>
    ///     Path to a browser executable to run instead of the bundled one.
    /// </summary>
    public string? ExecutablePath { get; set; }

    /// <summary>
    ///     Whether to enable the Chromium sandbox.
    /// </summary>
    public bool? ChromiumSandbox { get; set; }

    /// <summary>
    ///     Environment variables visible to the browser.
    /// </summary>
    public Dictionary<string, string>? Env { get; set; }

    /// <summary>
    ///     Firefox user preferences. <c>true</c>/<c>false</c> and integers are converted to their type.
    /// </summary>
    public Dictionary<string, string>? FirefoxUserPrefs { get; set; }

    /// <summary>
    ///     Command line arguments.
    /// </summary>
    public ArgumentsSettings? Arguments { get; set; }

    /// <summary>
    ///     Directories used by the browser.
    /// </summary>
    public DirectoriesSettings? Directories { get; set; }

    /// <summary>
    ///     Process signals that close the browser.
    /// </summary>
    public SignalsSettings? Signals { get; set; }

    /// <summary>
    ///     Network proxy of the browser.
    /// </summary>
    public ProxySettings? Proxy { get; set; }

    /// <summary>
    ///     Build the Playwright launch options from these settings.
    /// </summary>
    /// <returns>Browser type launch options.</returns>
    public BrowserTypeLaunchOptions ToLaunchOptions()
    {
        var options = new BrowserTypeLaunchOptions();

        if (Headless != null) options.Headless = Headless;
        if (SlowMo != null) options.SlowMo = SlowMo;
        if (Timeout != null) options.Timeout = Timeout;
        if (Channel != null) options.Channel = Channel;
        if (ExecutablePath != null) options.ExecutablePath = ExecutablePath;
        if (ChromiumSandbox != null) options.ChromiumSandbox = ChromiumSandbox;
        if (Env != null) options.Env = Env;
        if (FirefoxUserPrefs != null)
            options.FirefoxUserPrefs = FirefoxUserPrefs.ToDictionary(pref => pref.Key, pref => ToPrefValue(pref.Value));
        if (Proxy != null) options.Proxy = Proxy.ToProxy();

        Arguments?.ApplyTo(options);
        Directories?.ApplyTo(options);
        Signals?.ApplyTo(options);

        return options;
    }

    private static object ToPrefValue(string value)
    {
        if (bool.TryParse(value, out bool boolean)) return boolean;
        if (int.TryParse(value, out int integer)) return integer;
        return value;
    }
}

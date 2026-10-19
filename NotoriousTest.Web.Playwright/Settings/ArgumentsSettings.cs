using Microsoft.Playwright;

namespace NotoriousTest.Web.Playwright.Settings;

/// <summary>
///     Command line arguments passed to the browser.
/// </summary>
public class ArgumentsSettings
{
    /// <summary>
    ///     Additional arguments to pass to the browser instance.
    /// </summary>
    public string[]? Args { get; set; }

    /// <summary>
    ///     Default Playwright arguments to filter out.
    /// </summary>
    public string[]? IgnoreDefaultArgs { get; set; }

    /// <summary>
    ///     Whether to filter out all the default Playwright arguments.
    /// </summary>
    public bool? IgnoreAllDefaultArgs { get; set; }

    internal void ApplyTo(BrowserTypeLaunchOptions options)
    {
        if (Args != null) options.Args = Args;
        if (IgnoreDefaultArgs != null) options.IgnoreDefaultArgs = IgnoreDefaultArgs;
        if (IgnoreAllDefaultArgs != null) options.IgnoreAllDefaultArgs = IgnoreAllDefaultArgs;
    }
}

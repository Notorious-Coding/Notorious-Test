using Microsoft.Playwright;

namespace NotoriousTest.Web.Playwright.Settings;

/// <summary>
///     Process signals that close the browser.
/// </summary>
public class SignalsSettings
{
    /// <summary>
    ///     Whether to close the browser process on Ctrl-C.
    /// </summary>
    public bool? HandleSIGINT { get; set; }

    /// <summary>
    ///     Whether to close the browser process on SIGTERM.
    /// </summary>
    public bool? HandleSIGTERM { get; set; }

    /// <summary>
    ///     Whether to close the browser process on SIGHUP.
    /// </summary>
    public bool? HandleSIGHUP { get; set; }

    internal void ApplyTo(BrowserTypeLaunchOptions options)
    {
        if (HandleSIGINT != null) options.HandleSIGINT = HandleSIGINT;
        if (HandleSIGTERM != null) options.HandleSIGTERM = HandleSIGTERM;
        if (HandleSIGHUP != null) options.HandleSIGHUP = HandleSIGHUP;
    }
}

using Microsoft.Playwright;

namespace NotoriousTest.Web.Playwright.Settings;

/// <summary>
///     Directories used by the browser.
/// </summary>
public class DirectoriesSettings
{
    /// <summary>
    ///     Directory where the accepted downloads are put.
    /// </summary>
    public string? Downloads { get; set; }

    /// <summary>
    ///     Directory where the traces are saved.
    /// </summary>
    public string? Traces { get; set; }

    /// <summary>
    ///     Directory where the browser artifacts are saved.
    /// </summary>
    public string? Artifacts { get; set; }

    internal void ApplyTo(BrowserTypeLaunchOptions options)
    {
        if (Downloads != null) options.DownloadsPath = Downloads;
        if (Traces != null) options.TracesDir = Traces;
        if (Artifacts != null) options.ArtifactsDir = Artifacts;
    }
}

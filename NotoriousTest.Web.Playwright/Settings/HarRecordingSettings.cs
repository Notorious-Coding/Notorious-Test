using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace NotoriousTest.Web.Playwright.Settings;

/// <summary>
///     HAR recording of the browser context.
/// </summary>
public class HarRecordingSettings
{
    /// <summary>
    ///     Path on the filesystem to write the HAR file to.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    ///     Resource content management (Omit, Embed, Attach).
    /// </summary>
    public HarContentPolicy? Content { get; set; }

    /// <summary>
    ///     Recording mode (Full, Minimal).
    /// </summary>
    public HarMode? Mode { get; set; }

    /// <summary>
    ///     Whether to omit request content from the HAR.
    /// </summary>
    public bool? OmitContent { get; set; }

    /// <summary>
    ///     Glob pattern of the requests to store in the HAR.
    /// </summary>
    public string? UrlFilter { get; set; }

    /// <summary>
    ///     Glob pattern of the requests to store in the HAR.
    /// </summary>
    public string? UrlFilterString { get; set; }

    /// <summary>
    ///     Regular expression of the requests to store in the HAR.
    /// </summary>
    public string? UrlFilterRegex { get; set; }

    internal void ApplyTo(BrowserNewContextOptions options)
    {
        if (Path != null) options.RecordHarPath = Path;
        if (Content != null) options.RecordHarContent = Content;
        if (Mode != null) options.RecordHarMode = Mode;
        if (OmitContent != null) options.RecordHarOmitContent = OmitContent;
        if (UrlFilter != null) options.RecordHarUrlFilter = UrlFilter;
        if (UrlFilterString != null) options.RecordHarUrlFilterString = UrlFilterString;
        if (UrlFilterRegex != null) options.RecordHarUrlFilterRegex = new Regex(UrlFilterRegex);
    }
}

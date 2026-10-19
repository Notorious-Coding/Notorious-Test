using Microsoft.Playwright;

namespace NotoriousTest.Web.Playwright.Settings;

/// <summary>
///     Video recording of the pages of the browser context.
/// </summary>
public class VideoRecordingSettings
{
    /// <summary>
    ///     Directory to put the videos into.
    /// </summary>
    public string? Dir { get; set; }

    /// <summary>
    ///     Size of the recorded videos. Defaults to the viewport scaled down to fit into 800x800.
    /// </summary>
    public SizeSettings? Size { get; set; }

    internal void ApplyTo(BrowserNewContextOptions options)
    {
        if (Dir != null) options.RecordVideoDir = Dir;
        if (Size != null) options.RecordVideoSize = new RecordVideoSize { Width = Size.Width, Height = Size.Height };
    }
}

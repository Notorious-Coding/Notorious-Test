using Microsoft.Playwright;

namespace NotoriousTest.Web.Playwright.Settings;

/// <summary>
///     HAR and video recording of the browser context.
/// </summary>
public class RecordingSettings
{
    /// <summary>
    ///     HAR recording.
    /// </summary>
    public HarRecordingSettings? Har { get; set; }

    /// <summary>
    ///     Video recording.
    /// </summary>
    public VideoRecordingSettings? Video { get; set; }

    internal void ApplyTo(BrowserNewContextOptions options)
    {
        Har?.ApplyTo(options);
        Video?.ApplyTo(options);
    }
}

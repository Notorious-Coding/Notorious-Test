using Microsoft.Playwright;

namespace NotoriousTest.Web.Playwright.Settings;

/// <summary>
///     Emulated device of the browser context.
/// </summary>
public class DeviceSettings
{
    /// <summary>
    ///     Viewport size of each page. Ignored when <see cref="NoViewport" /> is true.
    /// </summary>
    public SizeSettings? ViewportSize { get; set; }

    /// <summary>
    ///     Disable the consistent viewport emulation.
    /// </summary>
    public bool? NoViewport { get; set; }

    /// <summary>
    ///     Screen size available to the web page through <c>window.screen</c>.
    /// </summary>
    public SizeSettings? ScreenSize { get; set; }

    /// <summary>
    ///     Device scale factor (can be thought of as dpr).
    /// </summary>
    public float? DeviceScaleFactor { get; set; }

    /// <summary>
    ///     Whether the <c>meta viewport</c> tag is taken into account and touch events are enabled.
    /// </summary>
    public bool? IsMobile { get; set; }

    /// <summary>
    ///     Whether the viewport supports touch events.
    /// </summary>
    public bool? HasTouch { get; set; }

    /// <summary>
    ///     User agent to use in this context.
    /// </summary>
    public string? UserAgent { get; set; }

    internal void ApplyTo(BrowserNewContextOptions options)
    {
        if (NoViewport == true)
            options.ViewportSize = Microsoft.Playwright.ViewportSize.NoViewport;
        else if (ViewportSize != null)
            options.ViewportSize = new ViewportSize { Width = ViewportSize.Width, Height = ViewportSize.Height };

        if (ScreenSize != null)
            options.ScreenSize = new ScreenSize { Width = ScreenSize.Width, Height = ScreenSize.Height };

        if (DeviceScaleFactor != null) options.DeviceScaleFactor = DeviceScaleFactor;
        if (IsMobile != null) options.IsMobile = IsMobile;
        if (HasTouch != null) options.HasTouch = HasTouch;
        if (UserAgent != null) options.UserAgent = UserAgent;
    }
}

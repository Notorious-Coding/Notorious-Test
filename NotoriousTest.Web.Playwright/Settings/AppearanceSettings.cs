using Microsoft.Playwright;

namespace NotoriousTest.Web.Playwright.Settings;

/// <summary>
///     Emulated media features of the browser context.
/// </summary>
public class AppearanceSettings
{
    /// <summary>
    ///     <c>prefers-color-scheme</c> media feature (Light, Dark, NoPreference, Null).
    /// </summary>
    public ColorScheme? ColorScheme { get; set; }

    /// <summary>
    ///     <c>prefers-contrast</c> media feature (NoPreference, More, Null).
    /// </summary>
    public Contrast? Contrast { get; set; }

    /// <summary>
    ///     <c>forced-colors</c> media feature (Active, None, Null).
    /// </summary>
    public ForcedColors? ForcedColors { get; set; }

    /// <summary>
    ///     <c>prefers-reduced-motion</c> media feature (Reduce, NoPreference, Null).
    /// </summary>
    public ReducedMotion? ReducedMotion { get; set; }

    internal void ApplyTo(BrowserNewContextOptions options)
    {
        if (ColorScheme != null) options.ColorScheme = ColorScheme;
        if (Contrast != null) options.Contrast = Contrast;
        if (ForcedColors != null) options.ForcedColors = ForcedColors;
        if (ReducedMotion != null) options.ReducedMotion = ReducedMotion;
    }
}

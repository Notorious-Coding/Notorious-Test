using Microsoft.Playwright;

namespace NotoriousTest.Web.Playwright.Settings;

/// <summary>
///     Locale, timezone and geolocation of the browser context.
/// </summary>
public class LocalizationSettings
{
    /// <summary>
    ///     User locale, for example <c>en-GB</c> or <c>fr-FR</c>.
    /// </summary>
    public string? Locale { get; set; }

    /// <summary>
    ///     Timezone of the context, for example <c>Europe/Paris</c>.
    /// </summary>
    public string? TimezoneId { get; set; }

    /// <summary>
    ///     Emulated geolocation.
    /// </summary>
    public GeolocationSettings? Geolocation { get; set; }

    internal void ApplyTo(BrowserNewContextOptions options)
    {
        if (Locale != null) options.Locale = Locale;
        if (TimezoneId != null) options.TimezoneId = TimezoneId;
        if (Geolocation != null)
            options.Geolocation = new Geolocation
            {
                Latitude = Geolocation.Latitude, Longitude = Geolocation.Longitude, Accuracy = Geolocation.Accuracy
            };
    }
}

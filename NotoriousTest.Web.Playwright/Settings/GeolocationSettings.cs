namespace NotoriousTest.Web.Playwright.Settings;

/// <summary>
///     Emulated geolocation.
/// </summary>
public class GeolocationSettings
{
    /// <summary>
    ///     Latitude between -90 and 90.
    /// </summary>
    public float Latitude { get; set; }

    /// <summary>
    ///     Longitude between -180 and 180.
    /// </summary>
    public float Longitude { get; set; }

    /// <summary>
    ///     Non-negative accuracy value. Defaults to 0.
    /// </summary>
    public float? Accuracy { get; set; }
}

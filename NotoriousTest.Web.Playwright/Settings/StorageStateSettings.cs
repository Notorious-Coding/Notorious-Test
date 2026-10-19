using Microsoft.Playwright;

namespace NotoriousTest.Web.Playwright.Settings;

/// <summary>
///     Storage state (cookies and local storage) the browser context is populated with.
/// </summary>
public class StorageStateSettings
{
    /// <summary>
    ///     Storage state as a JSON string.
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    ///     Path to the file with the saved storage state.
    /// </summary>
    public string? Path { get; set; }

    internal void ApplyTo(BrowserNewContextOptions options)
    {
        if (State != null) options.StorageState = State;
        if (Path != null) options.StorageStatePath = Path;
    }
}

using NotoriousTest.Core;
using NotoriousTest.Core.Infrastructures;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Settings;

namespace NotoriousTest.Web.Playwright.Browsers;

public class BrowserSettings
{
    public static string SECTION_KEY = "Browser";
    public string Type { get; set; }
}

/// <summary>
///
/// </summary>
public class SettingsBrowserInfrastructure : BrowserInfrastructure
{
    /// <summary>
    ///     Browser settings.
    /// </summary>
    /// <exception cref="InfrastructureSettingsNotFound"></exception>
    protected BrowserSettings Settings => _settings.Get<BrowserSettings>(BrowserSettings.SECTION_KEY) ??
                                          throw new InfrastructureSettingsNotFound(
                                              $"testsettings.json does not contain a {BrowserSettings.SECTION_KEY} section.");

    /// <summary>
    ///     Browser type found in settings (e.g., Chromium, Firefox, Webkit).
    /// </summary>
    /// <exception cref="InfrastructureSettingsNotFound"></exception>
    protected override BrowserType BrowserType => Settings.Type ??
                                                  throw new InfrastructureSettingsNotFound(
                                                      $"Browser type not found in section {BrowserSettings.SECTION_KEY}.");


    private readonly ITestSettingsProvider _settings;

    public SettingsBrowserInfrastructure(ITestSettingsProvider settings, EnvironmentId contextId, ITestLogger logger,
        IRegistry provider) : base(
        contextId, logger, provider)
    {
        _settings = settings;
        Dependencies.Add(CreateDependency(BrowserType));
    }

    private static PlaywrightBrowserDependency CreateDependency(BrowserType browserType)
    {
        if (browserType == BrowserType.Chromium) return new ChromiumDependency();
        if (browserType == BrowserType.Firefox) return new FirefoxDependency();
        if (browserType == BrowserType.Webkit) return new WebkitDependency();

        throw new ArgumentException(
            $"Unsupported browser: {browserType}. " +
            $"Supported: {BrowserType.Chromium}, {BrowserType.Firefox}, {BrowserType.Webkit}.");
    }
}

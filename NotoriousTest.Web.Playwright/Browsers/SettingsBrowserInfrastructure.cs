using NotoriousTest.Core;
using NotoriousTest.Core.Infrastructures;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Settings;
using NotoriousTest.Web.Playwright.Settings;

namespace NotoriousTest.Web.Playwright.Browsers;

/// <summary>
///     Create the browser configured in the testsettings.json file for a web application.
/// </summary>
public class SettingsBrowserInfrastructure : BrowserInfrastructure
{
    /// <summary>
    ///     Browser type found in settings (e.g., Chromium, Firefox, Webkit).
    /// </summary>
    /// <exception cref="InfrastructureSettingsNotFound"></exception>
    protected override BrowserType BrowserType => BrowserSettings?.Type ??
                                                  throw new InfrastructureSettingsNotFound(
                                                      $"Browser type not found in section {BrowserSettings.SECTION_KEY}.");

    /// <inheritdoc />
    public SettingsBrowserInfrastructure(ITestSettingsProvider settings, EnvironmentId contextId, ITestLogger logger,
        IRegistry provider) : base(
        settings, contextId, logger, provider)
    {
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

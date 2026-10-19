using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Settings;

namespace NotoriousTest.Web.Playwright.Browsers;

/// <summary>
///     Create a firefox browser infrastructure for a web application.
/// </summary>
public class FirefoxBrowserInfrastructure : BrowserInfrastructure
{
    protected override BrowserType BrowserType => BrowserType.Firefox;

    /// <inheritdoc />
    public FirefoxBrowserInfrastructure(ITestSettingsProvider settings, EnvironmentId contextId, ITestLogger logger,
        IRegistry provider) : base(
        settings, contextId, logger, provider)
    {
        Dependencies.Add(new FirefoxDependency());
    }
}

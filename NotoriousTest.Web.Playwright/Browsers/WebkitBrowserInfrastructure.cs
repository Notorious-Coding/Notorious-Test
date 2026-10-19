using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Settings;

namespace NotoriousTest.Web.Playwright.Browsers;

/// <summary>
///     Infrastructure for webkit browser.
/// </summary>
public class WebkitBrowserInfrastructure : BrowserInfrastructure
{
    /// <inheritdoc />
    protected override BrowserType BrowserType => BrowserType.Webkit;

    /// <inheritdoc />
    public WebkitBrowserInfrastructure(ITestSettingsProvider settings, EnvironmentId contextId, ITestLogger logger,
        IRegistry provider) : base(
        settings, contextId, logger, provider)
    {
        Dependencies.Add(new WebkitDependency());
    }
}

using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;

namespace NotoriousTest.Web.Playwright.Browsers;

/// <summary>
///     Create a chromium browser infrastructure for a web application.
/// </summary>
public class ChromiumBrowserInfrastructure : BrowserInfrastructure
{
    /// <inheritdoc />
    protected override BrowserType BrowserType => BrowserType.Chromium;

    /// <inheritdoc />
    public ChromiumBrowserInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry provider) : base(
        contextId, logger, provider)
    {
        Dependencies.Add(new ChromiumDependency());
    }
}

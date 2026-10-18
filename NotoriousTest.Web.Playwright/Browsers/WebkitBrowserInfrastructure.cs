using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;

namespace NotoriousTest.Web.Playwright.Browsers;

/// <summary>
///     Infrastructure for webkit browser.
/// </summary>
public class WebkitBrowserInfrastructure : BrowserInfrastructure
{
    /// <inheritdoc />
    protected override BrowserType BrowserType => BrowserType.Webkit;

    /// <inheritdoc />
    public WebkitBrowserInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry provider) : base(
        contextId, logger, provider)
    {
        Dependencies.Add(new WebkitDependency());
    }
}

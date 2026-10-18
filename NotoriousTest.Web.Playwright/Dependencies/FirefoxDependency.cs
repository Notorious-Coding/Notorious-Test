using NotoriousTest.Web.Playwright.Browsers;

namespace NotoriousTest.Web.Playwright;

/// <summary>
///     Ensures the Playwright-bundled Firefox is installed.
/// </summary>
public sealed class FirefoxDependency() : PlaywrightBrowserDependency(BrowserType.Firefox);

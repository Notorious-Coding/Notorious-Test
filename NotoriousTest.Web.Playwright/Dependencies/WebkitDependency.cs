using NotoriousTest.Web.Playwright.Browsers;

namespace NotoriousTest.Web.Playwright;

/// <summary>
///     Ensures the Playwright-bundled Webkit is installed.
/// </summary>
public sealed class WebkitDependency() : PlaywrightBrowserDependency(BrowserType.Webkit);

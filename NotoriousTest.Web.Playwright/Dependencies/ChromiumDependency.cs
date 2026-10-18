using NotoriousTest.Web.Playwright.Browsers;

namespace NotoriousTest.Web.Playwright;

/// <summary>
///     Ensures the Playwright-bundled Chromium is installed.
/// </summary>
public sealed class ChromiumDependency() : PlaywrightBrowserDependency(BrowserType.Chromium);

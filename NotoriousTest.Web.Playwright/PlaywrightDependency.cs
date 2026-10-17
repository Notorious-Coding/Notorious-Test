using Microsoft.Playwright;
using NotoriousTest.Core.Infrastructures.Dependencies;

namespace NotoriousTest.Web.Playwright;

/// <summary>
///     Ensures the Playwright-bundled browsers are installed before the test campaign.
///     Only bundled browsers are supported (chromium, firefox, webkit): branded channels
///     such as msedge or chrome are system-wide installs and are not managed here.
/// </summary>
public sealed class PlaywrightDependency : IInfrastructureDependency
{
    private static readonly string[] SupportedBrowsers =
        [BrowserType.Chromium, BrowserType.Firefox, BrowserType.Webkit];

    private readonly string[] _browsers;

    public PlaywrightDependency(params string[] browsers)
    {
        _browsers = browsers.Length == 0 ? [BrowserType.Chromium] : browsers;

        string[] unsupported = _browsers.Except(SupportedBrowsers).ToArray();
        if (unsupported.Length > 0)
            throw new ArgumentException(
                $"Unsupported browser(s): {string.Join(", ", unsupported)}. " +
                $"Supported: {string.Join(", ", SupportedBrowsers)}.");
    }

    public async Task<bool> Exist()
    {
        // ExecutablePath is where this Playwright version expects the bundled browser.
        // Checking the file catches both "never installed" and "installed for another Playwright version".
        using IPlaywright playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        return _browsers.All(browser => File.Exists(playwright[browser].ExecutablePath));
    }

    public Task Install()
        => Task.Run(() => Run(["install", .. _browsers], "install"));

    public Task Uninstall()
        // Removes the browsers used by this Playwright version, unless another
        // Playwright installation on the machine still references them.
        => Task.Run(() => Run(["uninstall"], "uninstall"));

    private static void Run(string[] args, string operation)
    {
        int exitCode = Program.Main(args);
        if (exitCode != 0)
            throw new InvalidOperationException(
                $"Playwright browser {operation} failed (exit code {exitCode}). " +
                $"Command: playwright {string.Join(' ', args)}");
    }
}

using Microsoft.Playwright;
using NotoriousTest.Core.Infrastructures.Dependencies;
using BrowserType = NotoriousTest.Web.Playwright.Browsers.BrowserType;

namespace NotoriousTest.Web.Playwright;

/// <summary>
///     Ensures a Playwright-bundled browser is installed before the test campaign.
///     Each browser is installed independently, so each one is its own dependency.
///     Only bundled browsers are supported (chromium, firefox, webkit): branded channels
///     such as msedge or chrome are system-wide installation and are not managed here.
/// </summary>
public abstract class PlaywrightBrowserDependency : IInfrastructureDependency
{
    public BrowserType Browser { get; }

    protected PlaywrightBrowserDependency(BrowserType browser)
    {
        Browser = browser;
    }

    public async Task<bool> Exist()
    {
        using IPlaywright playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        return File.Exists(playwright[Browser].ExecutablePath);
    }

    public async Task Install()
    {
        if (!await Exist()) Run(["install", Browser]);
    }


    /// <summary>
    ///     Does nothing: installed browsers are intentionally left in place.
    /// </summary>
    /// <remarks>
    ///     Playwright browsers live in a machine-wide cache shared by every Playwright installation,
    ///     so removing them could break other projects, parallel runs or CI caches.
    ///     Playwright already removes unused browser revisions on each install.
    ///     To clean them up manually, run <c>playwright.ps1 uninstall</c>.
    /// </remarks>
    /// <returns>A completed task.</returns>
    public Task Uninstall() => Task.CompletedTask;

    // Two infrastructures using the same browser share the same dependency,
    // so the environment installs it only once.
    public override bool Equals(object? obj) => obj?.GetType() == GetType();

    public override int GetHashCode() => GetType().GetHashCode();

    private static void Run(string[] args)
    {
        int exitCode = Program.Main(args);
        if (exitCode != 0)
            throw new InvalidOperationException(
                $"Playwright browser {args[0]} failed (exit code {exitCode}). " +
                $"Command: playwright {string.Join(' ', args)}");
    }
}

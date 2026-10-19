using Microsoft.Playwright;
using NotoriousTest.Core.Environments;
using NotoriousTest.Web.Playwright.Browsers;

namespace NotoriousTest.Web.Playwright;

/// <summary>
///     Playwright environment extensions.
/// </summary>
public static class EnvironmentExtensions
{
    extension(EnvironmentBase environment)
    {
        /// <summary>
        ///     Get the first browser registered in the environment.
        /// </summary>
        public BrowserInfrastructure BrowserInfrastructure => environment.GetInfrastructure<BrowserInfrastructure>();

        /// <summary>
        ///     Playwright page used for navigation and assertions in the browser.
        /// </summary>
        public IPage Page => environment.BrowserInfrastructure.Page ??
                             throw new InvalidOperationException("No browser configured");

        /// <summary>
        ///     Playwright browser used to create a new isolated context.
        /// </summary>
        public IBrowser Browser => environment.BrowserInfrastructure.Browser ??
                                   throw new InvalidOperationException("No browser configured");

        /// <summary>
        ///     Playwright context used to create new pages.
        /// </summary>
        public IBrowserContext Context => environment.BrowserInfrastructure.Context ??
                                          throw new InvalidOperationException("No browser configured");

        /// <summary>
        ///     Add the browser configured in the testsettings.json file to the environment.
        /// </summary>
        public EnvironmentBase AddBrowserFromSettings() =>
            environment.AddInfrastructure<SettingsBrowserInfrastructure>();

        /// <summary>
        ///     Add a Chromium browser to the environment.
        /// </summary>
        /// <returns>Environment configured with a Chromium browser.</returns>
        public EnvironmentBase AddChromiumBrowser() => environment.AddInfrastructure<ChromiumBrowserInfrastructure>();

        /// <summary>
        ///     Add a Firefox browser to the environment.
        /// </summary>
        /// <returns>Environment configured with a Firefox browser.</returns>
        public EnvironmentBase AddFirefoxBrowser() => environment.AddInfrastructure<FirefoxBrowserInfrastructure>();

        /// <summary>
        ///     Add a Webkit browser to the environment.
        /// </summary>
        /// <returns>Environment configured with a Webkit browser.</returns>
        public EnvironmentBase AddWebkitBrowser() => environment.AddInfrastructure<WebkitBrowserInfrastructure>();
    }
}

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
    }

    extension(InfrastructureListBuilder builder)
    {
        /// <summary>
        ///     Add the browser configured in the testsettings.json file to the current group of infrastructures.
        /// </summary>
        public InfrastructureListBuilder AddBrowserFromSettings() => builder.Add<SettingsBrowserInfrastructure>();

        /// <summary>
        ///     Add the browser configured in the testsettings.json file in a new group, started after the previous ones.
        /// </summary>
        public InfrastructureListBuilder ThenAddBrowserFromSettings() =>
            builder.ThenAdd<SettingsBrowserInfrastructure>();

        /// <summary>
        ///     Add a Chromium browser to the current group of infrastructures.
        /// </summary>
        /// <returns>Builder configured with a Chromium browser.</returns>
        public InfrastructureListBuilder AddChromiumBrowser() => builder.Add<ChromiumBrowserInfrastructure>();

        /// <summary>
        ///     Add a Chromium browser in a new group, started after the previous ones.
        /// </summary>
        /// <returns>Builder configured with a Chromium browser.</returns>
        public InfrastructureListBuilder ThenAddChromiumBrowser() => builder.ThenAdd<ChromiumBrowserInfrastructure>();

        /// <summary>
        ///     Add a Firefox browser to the current group of infrastructures.
        /// </summary>
        /// <returns>Builder configured with a Firefox browser.</returns>
        public InfrastructureListBuilder AddFirefoxBrowser() => builder.Add<FirefoxBrowserInfrastructure>();

        /// <summary>
        ///     Add a Firefox browser in a new group, started after the previous ones.
        /// </summary>
        /// <returns>Builder configured with a Firefox browser.</returns>
        public InfrastructureListBuilder ThenAddFirefoxBrowser() => builder.ThenAdd<FirefoxBrowserInfrastructure>();

        /// <summary>
        ///     Add a Webkit browser to the current group of infrastructures.
        /// </summary>
        /// <returns>Builder configured with a Webkit browser.</returns>
        public InfrastructureListBuilder AddWebkitBrowser() => builder.Add<WebkitBrowserInfrastructure>();

        /// <summary>
        ///     Add a Webkit browser in a new group, started after the previous ones.
        /// </summary>
        /// <returns>Builder configured with a Webkit browser.</returns>
        public InfrastructureListBuilder ThenAddWebkitBrowser() => builder.ThenAdd<WebkitBrowserInfrastructure>();
    }
}

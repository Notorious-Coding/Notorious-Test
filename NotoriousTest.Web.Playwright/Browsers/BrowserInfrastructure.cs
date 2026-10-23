using Microsoft.Playwright;
using NotoriousTest.Core;
using NotoriousTest.Core.Configuration;
using NotoriousTest.Core.Infrastructures;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Settings;
using NotoriousTest.Web.Playwright.Settings;

namespace NotoriousTest.Web.Playwright.Browsers;

/// <summary>
///     Create a browser infrastructure for a web application.
/// </summary>
public abstract class BrowserInfrastructure : Infrastructure, IConfigurationConsumer
{
    /// <summary>
    ///     Playwright page.
    /// </summary>
    public IPage? Page { get; private set; }

    /// <summary>
    ///     Playwright browser.
    /// </summary>
    public IBrowser? Browser { get; private set; }

    /// <summary>
    ///     Playwright context.
    /// </summary>
    public IBrowserContext? Context { get; private set; }

    /// <inheritdoc />
    public List<ConfigurationEntry<object>> ConsumedConfiguration { get; set; }

    /// <summary>
    ///     Browser type that needs to be started (chromium, firefox, webkit).
    /// </summary>
    protected virtual BrowserType BrowserType { get; }

    /// <summary>
    ///     Browser settings found in the testsettings.json file, if any.
    /// </summary>
    protected BrowserSettings? BrowserSettings => _settings.Get<BrowserSettings>(BrowserSettings.SECTION_KEY);

    private readonly ITestSettingsProvider _settings;
    private bool _playwrightAcquired;


    /// <summary>
    ///     Create a instance of a browser infrastructure.
    /// </summary>
    /// <param name="settings">Test settings provider.</param>
    /// <param name="contextId">Current environment.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="provider">Registry.</param>
    public BrowserInfrastructure(ITestSettingsProvider settings, EnvironmentId contextId, ITestLogger logger,
        IRegistry provider) : base(contextId,
        logger, provider)
    {
        _settings = settings;
    }

    /// <inheritdoc />
    public override async Task Destroy()
    {
        // Closing the context first flushes the recorded videos and HAR.
        if (Context != null) await Context.CloseAsync();
        if (Browser != null) await Browser.CloseAsync();

        if (_playwrightAcquired)
        {
            _playwrightAcquired = false;
            await PlaywrightProvider.ReleaseAsync();
        }
    }

    /// <inheritdoc />
    public override async Task Initialize()
    {
        IPlaywright playwright = await PlaywrightProvider.AcquireAsync();
        _playwrightAcquired = true;

        Browser = await playwright[BrowserType].LaunchAsync(GetLaunchOptions());

        Context = await Browser.NewContextAsync(GetContextOptions());

        Page = await Context.NewPageAsync();
    }

    /// <inheritdoc />
    public override async Task Reset()
    {
        await Context.CloseAsync();
        Context = await Browser!.NewContextAsync(GetContextOptions());
        Page = await Context.NewPageAsync();
    }

    public async Task<IBrowserContext> CreateNewContext() => await Browser!.NewContextAsync(GetContextOptions());

    /// <summary>
    ///     Override this method to customize the launch options.
    ///     By default, the options come from the <c>Browser:Launch</c> section of the testsettings.json file.
    /// </summary>
    /// <returns>Browser type launch options.</returns>
    protected virtual BrowserTypeLaunchOptions GetLaunchOptions() =>
        (BrowserSettings ?? new BrowserSettings()).ToLaunchOptions();

    /// <summary>
    ///     Override this method to customize the context options.
    ///     By default, the options come from the <c>Browser</c> section of the testsettings.json file.
    /// </summary>
    /// <returns>Browser new context options.</returns>
    /// <exception cref="NoWebApplicationConfiguredException">
    ///     No web app url has been found in the consumed configuration.
    ///     Verify that a webapp is initialized before this infrastructure.
    /// </exception>
    protected virtual BrowserNewContextOptions GetContextOptions() =>
        (BrowserSettings ?? new BrowserSettings()).ToContextOptions(
            ConsumedConfiguration.WebAppUrl ?? throw new NoWebApplicationConfiguredException());

    /// <summary>
    ///     Exception thrown when no web application is configured.
    /// </summary>
    public class NoWebApplicationConfiguredException : Exception
    {
        /// <summary>
        ///     Create a new instance of the exception.
        /// </summary>
        public NoWebApplicationConfiguredException() : base(
            "No web application configured. Make sure to register a Web Application before starting a BrowserInfrastructure")
        {
        }
    }
}

using Microsoft.Playwright;
using NotoriousTest.Core;
using NotoriousTest.Core.Configuration;
using NotoriousTest.Core.Infrastructures;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;

namespace NotoriousTest.Web.Playwright.Browsers;

/// <summary>
///     Create a browser infrastructure for a web application.
/// </summary>
public abstract class BrowserInfrastructure : Infrastructure, IConfigurationConsumer
{
    /// <summary>
    ///     Playwright instance.
    /// </summary>
    internal static Lazy<Task<IPlaywright>>? Playwright;


    /// <inheritdoc />
    public override int? Order => 1000;

    /// <summary>
    ///     Playwright page.
    /// </summary>
    public IPage Page { get; private set; }

    /// <inheritdoc />
    public List<ConfigurationEntry<object>> ConsumedConfiguration { get; set; }

    /// <summary>
    ///     Browser type that need to be started (chromium, firefox, webkit).
    /// </summary>
    protected virtual BrowserType BrowserType { get; }

    private IBrowser? _browser;
    private IBrowserContext _context;

    /// <summary>
    ///     Create a instance of a browser infrastructure.
    /// </summary>
    /// <param name="contextId">Current environment.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="provider">Registry.</param>
    public BrowserInfrastructure(EnvironmentId contextId, ITestLogger logger,
        IRegistry provider) : base(contextId,
        logger, provider)
    {
    }

    /// <inheritdoc />
    public override async Task Destroy() => await _browser.CloseAsync();

    /// <inheritdoc />
    public override async Task Initialize()
    {
        Playwright ??= new Lazy<Task<IPlaywright>>(Microsoft.Playwright.Playwright.CreateAsync);

        _browser = await (await Playwright.Value)[BrowserType].LaunchAsync();

        _context = await _browser.NewContextAsync(GetContextOptions());

        Page = await _context.NewPageAsync();
    }

    /// <inheritdoc />
    public override async Task Reset()
    {
        await _context.CloseAsync();
        _context = await _browser!.NewContextAsync(GetContextOptions());
        Page = await _context.NewPageAsync();
    }

    /// <summary>
    ///     Override this method to customize the context options.
    /// </summary>
    /// <returns>Browser new context options.</returns>
    /// <exception cref="NoWebApplicationConfiguredException">
    ///     No web app url has been found in the consumed configuration.
    ///     Verify that a webapp is initialized before this infrastructure.
    /// </exception>
    protected virtual BrowserNewContextOptions GetContextOptions() => new()
    {
        BaseURL = ConsumedConfiguration.WebAppUrl ?? throw new NoWebApplicationConfiguredException()
    };

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

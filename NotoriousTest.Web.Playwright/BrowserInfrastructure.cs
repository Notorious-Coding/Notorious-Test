using Microsoft.Playwright;
using NotoriousTest.Core;
using NotoriousTest.Core.Configuration;
using NotoriousTest.Core.Infrastructures;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;

namespace NotoriousTest.Web.Playwright;

public class BrowserInfrastructure : Infrastructure, IConfigurationConsumer
{
    internal static Lazy<Task<IPlaywright>>? _playwright;
    internal IBrowser? _browser;
    private IBrowserContext _context;

    public BrowserInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry provider) : base(contextId,
        logger, provider)
    {
        Dependencies.Add(new PlaywrightDependency(BrowserType.Chromium));
    }

    public override int? Order => 1000;

    public IPage Page { get; private set; }
    public List<ConfigurationEntry<object>> ConsumedConfiguration { get; set; }

    protected virtual BrowserNewContextOptions GetContextOptions() => new()
    {
        BaseURL = ConsumedConfiguration.WebAppUrl ?? throw new NoWebApplicationConfiguredException()
    };

    public override async Task Destroy() => await _browser.CloseAsync();

    public override async Task Initialize()
    {
        _playwright ??= new Lazy<Task<IPlaywright>>(Microsoft.Playwright.Playwright.CreateAsync);

        _browser = await (await _playwright.Value).Chromium.LaunchAsync();

        _context = await _browser.NewContextAsync(GetContextOptions());

        Page = await _context.NewPageAsync();
    }

    public override async Task Reset()
    {
        await _context.CloseAsync();
        _context = await _browser!.NewContextAsync(GetContextOptions());
        Page = await _context.NewPageAsync();
    }

    public class NoWebApplicationConfiguredException : Exception
    {
        public NoWebApplicationConfiguredException() : base(
            "No web application configured. Make sure to register a Web Application before starting a BrowserInfrastructure")
        {
        }
    }
}

using System.Text.RegularExpressions;
using AwesomeAssertions;
using FakeItEasy;
using Microsoft.Playwright;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Settings;
using NotoriousTest.Web;
using NotoriousTest.Web.Playwright;
using NotoriousTest.Web.Playwright.Browsers;
using NotoriousTest.Web.Playwright.Settings;
using static Microsoft.Playwright.Assertions;

namespace NotoriousTest.IntegrationTests.Infrastructures.Playwright;

/// <summary>
///     Installs the Playwright browsers for the tests, as the environment would do.
/// </summary>
public class PlaywrightFixture : IAsyncLifetime
{
    private readonly ChromiumDependency dependency = new();
    private bool _installedByFixture;

    public async ValueTask InitializeAsync()
    {
        if (!await dependency.Exist())
        {
            await dependency.Install();
            _installedByFixture = true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_installedByFixture) await dependency.Uninstall();
    }
}

public class BrowserInfrastructureTests : IClassFixture<PlaywrightFixture>
{
    private const string WEB_APP_URL = "http://notorious.test";

    [Fact]
    public async Task Initialize_Should_CreateBrowser()
    {
        await using BrowserInfrastructure browser = CreateBrowser();

        await browser.InitializeAsync();
        await ServeFakePage(browser.Page);
        await browser.Page.GotoAsync("/");

        await Expect(browser.Page).ToHaveTitleAsync(new Regex("Notorious"));
    }

    [Fact]
    public async Task Destroy_Should_CloseBrowser()
    {
        BrowserInfrastructure browser = CreateBrowser();
        await browser.InitializeAsync();
        IBrowser playwrightBrowser = browser.Page.Context.Browser!;

        await browser.DestroyAsync();

        playwrightBrowser.IsConnected.Should().BeFalse();
        browser.Page.IsClosed.Should().BeTrue();
    }

    [Fact]
    public async Task Destroy_Should_ReleasePlaywright_When_LastBrowserIsDestroyed()
    {
        BrowserInfrastructure first = CreateBrowser();
        BrowserInfrastructure second = CreateBrowser();
        await first.InitializeAsync();
        await second.InitializeAsync();

        PlaywrightProvider.References.Should().Be(2);

        await first.DestroyAsync();
        PlaywrightProvider.References.Should().Be(1);

        await second.DestroyAsync();
        PlaywrightProvider.References.Should().Be(0);
    }

    [Fact]
    public async Task Destroy_Should_NotReleasePlaywright_When_BrowserWasNotInitialized()
    {
        BrowserInfrastructure initialized = CreateBrowser();
        await initialized.InitializeAsync();

        await CreateBrowser().DestroyAsync();

        PlaywrightProvider.References.Should().Be(1);
        await initialized.DestroyAsync();
    }

    [Fact]
    public async Task PlaywrightProvider_Should_ShareInstance_Until_LastRelease()
    {
        IPlaywright first = await PlaywrightProvider.AcquireAsync();
        IPlaywright second = await PlaywrightProvider.AcquireAsync();
        second.Should().BeSameAs(first);

        await PlaywrightProvider.ReleaseAsync();
        await PlaywrightProvider.ReleaseAsync();

        IPlaywright third = await PlaywrightProvider.AcquireAsync();
        third.Should().NotBeSameAs(first);
        await PlaywrightProvider.ReleaseAsync();
    }

    [Fact]
    public async Task Reset_Should_CreateNewPage()
    {
        await using BrowserInfrastructure browser = CreateBrowser();
        await browser.InitializeAsync();
        IPage previousPage = browser.Page;

        await browser.ResetAsync();

        previousPage.IsClosed.Should().BeTrue();
        browser.Page.Should().NotBeSameAs(previousPage);
        browser.Page.IsClosed.Should().BeFalse();
    }

    [Fact]
    public async Task Initialize_Should_ApplyContextOptionsFromSettings()
    {
        var settings = new BrowserSettings
        {
            Device = new DeviceSettings { ViewportSize = new SizeSettings { Width = 800, Height = 600 } },
            Localization = new LocalizationSettings { Locale = "fr-FR" }
        };
        await using BrowserInfrastructure browser = CreateBrowser(settings);

        await browser.InitializeAsync();
        await ServeFakePage(browser.Page);
        await browser.Page.GotoAsync("/");

        browser.Page.ViewportSize!.Width.Should().Be(800);
        browser.Page.ViewportSize.Height.Should().Be(600);
        (await browser.Page.EvaluateAsync<string>("navigator.language")).Should().Be("fr-FR");
    }

    [Fact]
    public async Task Initialize_Should_ApplyLaunchOptionsFromSettings()
    {
        var settings = new BrowserSettings
        {
            Launch = new LaunchSettings
            {
                Headless = true, Arguments = new ArgumentsSettings { Args = ["--user-agent=NotoriousLaunch"] }
            }
        };
        await using BrowserInfrastructure browser = CreateBrowser(settings);

        await browser.InitializeAsync();
        await ServeFakePage(browser.Page!);
        await browser.Page!.GotoAsync("/");

        (await browser.Page.EvaluateAsync<string>("navigator.userAgent")).Should().Be("NotoriousLaunch");
    }

    private static BrowserInfrastructure CreateBrowser(BrowserSettings? browserSettings = null)
    {
        var settings = A.Fake<ITestSettingsProvider>();
        A.CallTo(() => settings.Get<BrowserSettings>(BrowserSettings.SECTION_KEY)).Returns(browserSettings);

        var browser = new ChromiumBrowserInfrastructure(settings, Guid.NewGuid(), A.Fake<ITestLogger>(),
            A.Fake<IRegistry>())
        {
            ConsumedConfiguration = []
        };
        // Done by the environment from the WebApplicationInfrastructure output.
        browser.ConsumedConfiguration.WebAppUrl = WEB_APP_URL;
        return browser;
    }

    private static Task ServeFakePage(IPage page) =>
        page.RouteAsync($"{WEB_APP_URL}/**",
            route => route.FulfillAsync(new RouteFulfillOptions
            {
                ContentType = "text/html", Body = "<html><head><title>Notorious</title></head><body></body></html>"
            }));
}

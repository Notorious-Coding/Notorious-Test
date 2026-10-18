using System.Text.RegularExpressions;
using AwesomeAssertions;
using FakeItEasy;
using Microsoft.Playwright;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Web;
using NotoriousTest.Web.Playwright;
using NotoriousTest.Web.Playwright.Browsers;
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

    private static BrowserInfrastructure CreateBrowser()
    {
        var browser = new ChromiumBrowserInfrastructure(Guid.NewGuid(), A.Fake<ITestLogger>(), A.Fake<IRegistry>())
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

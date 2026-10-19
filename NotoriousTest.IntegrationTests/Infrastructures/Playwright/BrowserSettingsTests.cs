using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using NotoriousTest.Web.Playwright.Settings;

namespace NotoriousTest.IntegrationTests.Infrastructures.Playwright;

public class BrowserSettingsTests
{
    private const string BASE_URL = "http://notorious.test";

    [Fact]
    public void ToContextOptions_Should_OnlySetBaseUrl_When_SettingsAreEmpty()
    {
        BrowserNewContextOptions options = new BrowserSettings().ToContextOptions(BASE_URL);

        options.Should().BeEquivalentTo(new BrowserNewContextOptions { BaseURL = BASE_URL });
    }

    [Fact]
    public void ToContextOptions_Should_MapEverySetting()
    {
        var settings = new BrowserSettings
        {
            Device = new DeviceSettings
            {
                ViewportSize = new SizeSettings { Width = 1280, Height = 720 },
                ScreenSize = new SizeSettings { Width = 1920, Height = 1080 },
                DeviceScaleFactor = 2,
                IsMobile = true,
                HasTouch = true,
                UserAgent = "Notorious"
            },
            Localization = new LocalizationSettings
            {
                Locale = "fr-FR",
                TimezoneId = "Europe/Paris",
                Geolocation = new GeolocationSettings { Latitude = 48.85f, Longitude = 2.35f, Accuracy = 10 }
            },
            Appearance = new AppearanceSettings
            {
                ColorScheme = ColorScheme.Dark,
                Contrast = Contrast.More,
                ForcedColors = ForcedColors.Active,
                ReducedMotion = ReducedMotion.Reduce
            },
            Http = new HttpSettings
            {
                ExtraHTTPHeaders = new Dictionary<string, string> { ["X-Test"] = "1" },
                Credentials = new HttpCredentialsSettings
                {
                    Username = "user", Password = "pwd", Origin = BASE_URL, Send = HttpCredentialsSend.Always
                },
                CredentialsList = [new HttpCredentialsSettings { Username = "other", Password = "pwd" }],
                Proxy = new ProxySettings { Server = "http://proxy:3128", Bypass = ".com" },
                ClientCertificates = [new ClientCertificateSettings { Origin = BASE_URL, PfxPath = "cert.pfx" }],
                IgnoreHTTPSErrors = true,
                Offline = true
            },
            Security = new SecuritySettings
            {
                Permissions = ["geolocation"],
                BypassCSP = true,
                JavaScriptEnabled = false,
                AcceptDownloads = false,
                ServiceWorkers = ServiceWorkerPolicy.Block
            },
            Recording = new RecordingSettings
            {
                Har = new HarRecordingSettings
                {
                    Path = "record.har",
                    Content = HarContentPolicy.Embed,
                    Mode = HarMode.Minimal,
                    OmitContent = true,
                    UrlFilter = "**/api/**",
                    UrlFilterString = "**/api/**",
                    UrlFilterRegex = "api"
                },
                Video = new VideoRecordingSettings { Dir = "videos", Size = new SizeSettings { Width = 640, Height = 480 } }
            },
            StorageState = new StorageStateSettings { State = "{}", Path = "state.json" },
            StrictSelectors = true
        };

        BrowserNewContextOptions options = settings.ToContextOptions(BASE_URL);

        options.BaseURL.Should().Be(BASE_URL);
        options.ViewportSize.Should().BeEquivalentTo(new ViewportSize { Width = 1280, Height = 720 });
        options.ScreenSize.Should().BeEquivalentTo(new ScreenSize { Width = 1920, Height = 1080 });
        options.DeviceScaleFactor.Should().Be(2);
        options.IsMobile.Should().BeTrue();
        options.HasTouch.Should().BeTrue();
        options.UserAgent.Should().Be("Notorious");
        options.Locale.Should().Be("fr-FR");
        options.TimezoneId.Should().Be("Europe/Paris");
        options.Geolocation.Should().BeEquivalentTo(new Geolocation { Latitude = 48.85f, Longitude = 2.35f, Accuracy = 10 });
        options.ColorScheme.Should().Be(ColorScheme.Dark);
        options.Contrast.Should().Be(Contrast.More);
        options.ForcedColors.Should().Be(ForcedColors.Active);
        options.ReducedMotion.Should().Be(ReducedMotion.Reduce);
        options.ExtraHTTPHeaders.Should().BeEquivalentTo(new Dictionary<string, string> { ["X-Test"] = "1" });
        options.HttpCredentials.Should().BeEquivalentTo(new HttpCredentials
        {
            Username = "user", Password = "pwd", Origin = BASE_URL, Send = HttpCredentialsSend.Always
        });
        options.HttpCredentialsList.Should().ContainSingle().Which.Username.Should().Be("other");
        options.Proxy.Should().BeEquivalentTo(new Proxy { Server = "http://proxy:3128", Bypass = ".com" });
        options.ClientCertificates.Should().ContainSingle().Which.PfxPath.Should().Be("cert.pfx");
        options.IgnoreHTTPSErrors.Should().BeTrue();
        options.Offline.Should().BeTrue();
        options.Permissions.Should().Equal("geolocation");
        options.BypassCSP.Should().BeTrue();
        options.JavaScriptEnabled.Should().BeFalse();
        options.AcceptDownloads.Should().BeFalse();
        options.ServiceWorkers.Should().Be(ServiceWorkerPolicy.Block);
        options.RecordHarPath.Should().Be("record.har");
        options.RecordHarContent.Should().Be(HarContentPolicy.Embed);
        options.RecordHarMode.Should().Be(HarMode.Minimal);
        options.RecordHarOmitContent.Should().BeTrue();
        options.RecordHarUrlFilter.Should().Be("**/api/**");
        options.RecordHarUrlFilterString.Should().Be("**/api/**");
        options.RecordHarUrlFilterRegex!.ToString().Should().Be("api");
        options.RecordVideoDir.Should().Be("videos");
        options.RecordVideoSize.Should().BeEquivalentTo(new RecordVideoSize { Width = 640, Height = 480 });
        options.StorageState.Should().Be("{}");
        options.StorageStatePath.Should().Be("state.json");
        options.StrictSelectors.Should().BeTrue();
    }

    [Fact]
    public void ToContextOptions_Should_DisableViewport_When_NoViewportIsTrue()
    {
        var settings = new BrowserSettings
        {
            Device = new DeviceSettings
            {
                NoViewport = true, ViewportSize = new SizeSettings { Width = 1280, Height = 720 }
            }
        };

        BrowserNewContextOptions options = settings.ToContextOptions(BASE_URL);

        options.ViewportSize.Should().BeEquivalentTo(ViewportSize.NoViewport);
    }

    [Fact]
    public void ToLaunchOptions_Should_ReturnDefaults_When_LaunchIsMissing()
    {
        BrowserTypeLaunchOptions options = new BrowserSettings().ToLaunchOptions();

        options.Should().BeEquivalentTo(new BrowserTypeLaunchOptions());
    }

    [Fact]
    public void ToLaunchOptions_Should_MapEverySetting()
    {
        var settings = new BrowserSettings
        {
            Launch = new LaunchSettings
            {
                Headless = false,
                SlowMo = 200,
                Timeout = 30000,
                Channel = "msedge",
                ExecutablePath = "browser.exe",
                ChromiumSandbox = true,
                Env = new Dictionary<string, string> { ["DEBUG"] = "pw:api" },
                FirefoxUserPrefs = new Dictionary<string, string>
                {
                    ["bool.pref"] = "true", ["int.pref"] = "42", ["string.pref"] = "value"
                },
                Arguments = new ArgumentsSettings
                {
                    Args = ["--start-maximized"], IgnoreDefaultArgs = ["--mute-audio"], IgnoreAllDefaultArgs = false
                },
                Directories = new DirectoriesSettings { Downloads = "dl", Traces = "traces", Artifacts = "artifacts" },
                Signals = new SignalsSettings { HandleSIGINT = false, HandleSIGTERM = false, HandleSIGHUP = false },
                Proxy = new ProxySettings { Server = "http://proxy:3128", Username = "user" }
            }
        };

        BrowserTypeLaunchOptions options = settings.ToLaunchOptions();

        options.Headless.Should().BeFalse();
        options.SlowMo.Should().Be(200);
        options.Timeout.Should().Be(30000);
        options.Channel.Should().Be("msedge");
        options.ExecutablePath.Should().Be("browser.exe");
        options.ChromiumSandbox.Should().BeTrue();
        options.Env.Should().BeEquivalentTo(new Dictionary<string, string> { ["DEBUG"] = "pw:api" });
        options.FirefoxUserPrefs.Should().BeEquivalentTo(new Dictionary<string, object>
        {
            ["bool.pref"] = true, ["int.pref"] = 42, ["string.pref"] = "value"
        });
        options.Args.Should().Equal("--start-maximized");
        options.IgnoreDefaultArgs.Should().Equal("--mute-audio");
        options.IgnoreAllDefaultArgs.Should().BeFalse();
        options.DownloadsPath.Should().Be("dl");
        options.TracesDir.Should().Be("traces");
        options.ArtifactsDir.Should().Be("artifacts");
        options.HandleSIGINT.Should().BeFalse();
        options.HandleSIGTERM.Should().BeFalse();
        options.HandleSIGHUP.Should().BeFalse();
        options.Proxy.Should().BeEquivalentTo(new Proxy { Server = "http://proxy:3128", Username = "user" });
    }

    [Fact]
    public void Settings_Should_BeBoundFromConfiguration()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Browser:Type"] = "firefox",
                ["Browser:Device:ViewportSize:Width"] = "800",
                ["Browser:Device:ViewportSize:Height"] = "600",
                ["Browser:Appearance:ColorScheme"] = "Dark",
                ["Browser:Http:ExtraHTTPHeaders:X-Test"] = "1",
                ["Browser:Security:Permissions:0"] = "geolocation",
                ["Browser:StrictSelectors"] = "true",
                ["Browser:Launch:Headless"] = "false",
                ["Browser:Launch:Arguments:Args:0"] = "--start-maximized",
                ["Browser:Launch:Env:DEBUG"] = "pw:api"
            })
            .Build();
        var settings = new BrowserSettings();

        configuration.GetSection(BrowserSettings.SECTION_KEY).Bind(settings);
        BrowserNewContextOptions options = settings.ToContextOptions(BASE_URL);
        BrowserTypeLaunchOptions launchOptions = settings.ToLaunchOptions();

        launchOptions.Headless.Should().BeFalse();
        launchOptions.Args.Should().Equal("--start-maximized");
        launchOptions.Env.Should().BeEquivalentTo(new Dictionary<string, string> { ["DEBUG"] = "pw:api" });

        settings.Type.Should().Be("firefox");
        options.ViewportSize.Should().BeEquivalentTo(new ViewportSize { Width = 800, Height = 600 });
        options.ColorScheme.Should().Be(ColorScheme.Dark);
        options.ExtraHTTPHeaders.Should().BeEquivalentTo(new Dictionary<string, string> { ["X-Test"] = "1" });
        options.Permissions.Should().Equal("geolocation");
        options.StrictSelectors.Should().BeTrue();
    }
}

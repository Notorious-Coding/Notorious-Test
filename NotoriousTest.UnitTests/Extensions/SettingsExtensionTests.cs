using FakeItEasy;

using Microsoft.Extensions.Configuration;

using NotoriousTest.Extensions;
using NotoriousTest.Infrastructures;
using NotoriousTest.Logger;
using NotoriousTest.Settings;

namespace NotoriousTest.UnitTests.Extensions;

public class SettingsExtensionTests
{
    private class MySettings
    {
        public string? Host { get; set; }
        public int Port { get; set; }
    }

    private class MyInfrastructure : Infrastructure
    {
        public MyInfrastructure() : base(Guid.NewGuid(), A.Fake<ITestLogger>()) { }
        public override Task Initialize() => Task.CompletedTask;
        public override Task Reset() => Task.CompletedTask;
        public override Task Destroy() => Task.CompletedTask;
    }

    private class CustomSectionSettingsExtension : SettingsExtension<MySettings>
    {
        public CustomSectionSettingsExtension(ITestSettingsProvider provider) : base(provider)
        {
            SectionName = "CustomSection";
        }
    }

    private static ITestSettingsProvider BuildProvider(Dictionary<string, string?> values)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
        var provider = A.Fake<ITestSettingsProvider>();
        A.CallTo(() => provider.Find()).Returns(config);
        return provider;
    }

    [Fact]
    public void Settings_Should_BeInitializedToDefaultInstance()
    {
        var provider = A.Fake<ITestSettingsProvider>();
        var ext = new SettingsExtension<MySettings>(provider);

        Assert.NotNull(ext.Settings);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentNullException_WhenProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new SettingsExtension<MySettings>(null!));
    }

    [Fact]
    public async Task OnBeforeInitialize_Should_BindSettings_UsingInfrastructureTypeName_AsDefaultKey()
    {
        var provider = BuildProvider(new()
        {
            ["MyInfrastructure:Host"] = "localhost",
            ["MyInfrastructure:Port"] = "5432",
        });
        var ext = new SettingsExtension<MySettings>(provider);
        var infra = new MyInfrastructure();

        await ext.OnBeforeInitialize(infra);

        Assert.Equal("localhost", ext.Settings.Host);
        Assert.Equal(5432, ext.Settings.Port);
    }

    [Fact]
    public async Task OnBeforeInitialize_Should_BindSettings_UsingSectionName_WhenOverridden()
    {
        var provider = BuildProvider(new()
        {
            ["CustomSection:Host"] = "remotehost",
            ["CustomSection:Port"] = "1433",
        });
        var ext = new CustomSectionSettingsExtension(provider);
        var infra = new MyInfrastructure();

        await ext.OnBeforeInitialize(infra);

        Assert.Equal("remotehost", ext.Settings.Host);
        Assert.Equal(1433, ext.Settings.Port);
    }

    [Fact]
    public async Task OnBeforeInitialize_Should_LeaveDefaultSettings_WhenSectionDoesNotExist()
    {
        var provider = BuildProvider(new()
        {
            ["OtherSection:Host"] = "somehost",
        });
        var ext = new SettingsExtension<MySettings>(provider);
        var infra = new MyInfrastructure();

        await ext.OnBeforeInitialize(infra);

        Assert.Null(ext.Settings.Host);
        Assert.Equal(0, ext.Settings.Port);
    }

    [Fact]
    public async Task OnBeforeInitialize_Should_CallSettingsProvider()
    {
        var provider = BuildProvider(new());
        var ext = new SettingsExtension<MySettings>(provider);
        var infra = new MyInfrastructure();

        await ext.OnBeforeInitialize(infra);

        A.CallTo(() => provider.Find()).MustHaveHappenedOnceExactly();
    }
}

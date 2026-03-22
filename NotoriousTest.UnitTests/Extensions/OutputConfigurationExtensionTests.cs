using NotoriousTest.Extensions;
using NotoriousTest.Infrastructures;

namespace NotoriousTest.UnitTests.Extensions;

public class OutputConfigurationExtensionTests
{
    private class MyConfig
    {
        public string? Key1 { get; set; }
    }

    #region OutputConfigurationExtension direct tests

    [Fact]
    public void OutputConfiguration_Should_BeEmptyInitially()
    {
        var ext = new OutputConfigurationExtension<string>();

        Assert.Empty(ext.OutputConfiguration);
    }

    [Fact]
    public void AddEntry_WithKey_Should_AddEntryWithCorrectKeyAndValue()
    {
        var ext = new OutputConfigurationExtension<string>();

        ext.AddEntry("MyKey", "MyValue");

        Assert.Single(ext.OutputConfiguration);
        Assert.Equal("MyKey", ext.OutputConfiguration[0].Key);
        Assert.Equal("MyValue", ext.OutputConfiguration[0].Value);
    }

    [Fact]
    public void AddEntry_WithoutKey_Should_UseRuntimeTypeNameAsKey()
    {
        var ext = new OutputConfigurationExtension<MyConfig>();

        ext.AddEntry(new MyConfig());

        Assert.Single(ext.OutputConfiguration);
        Assert.Equal(nameof(MyConfig), ext.OutputConfiguration[0].Key);
    }

    [Fact]
    public void AddEntry_WithoutKey_OnObjectExtension_Should_UseRuntimeTypeNameAsKey()
    {
        var ext = new OutputConfigurationExtension<object>();

        ext.AddEntry(new MyConfig());

        Assert.Equal(nameof(MyConfig), ext.OutputConfiguration[0].Key);
    }

    [Fact]
    public void AddEntry_Multiple_Should_PreserveInsertionOrder()
    {
        var ext = new OutputConfigurationExtension<string>();

        ext.AddEntry("A", "first");
        ext.AddEntry("B", "second");
        ext.AddEntry("C", "third");

        Assert.Equal(3, ext.OutputConfiguration.Count);
        Assert.Equal("A", ext.OutputConfiguration[0].Key);
        Assert.Equal("B", ext.OutputConfiguration[1].Key);
        Assert.Equal("C", ext.OutputConfiguration[2].Key);
    }

    #endregion

    #region Integration with Infrastructure<T>

    private class TestInfrastructure : Infrastructure<string>
    {
        private readonly string key;
        private readonly string value;

        public TestInfrastructure(string key, string value) : base(Guid.NewGuid())
        {
            this.key = key;
            this.value = value;
        }


        public override Task Initialize()
        {
            AddEntry(key, value);
            return Task.CompletedTask;
        }

        public override Task Reset() => Task.CompletedTask;
        public override Task Destroy() => Task.CompletedTask;
    }

    [Fact]
    public async Task Infrastructure_AddEntry_Should_AppearInOutputConfiguration()
    {
        var infra = new TestInfrastructure("ConnectionString", "Server=localhost");

        await infra.Initialize();

        Assert.Single(infra.OutputConfiguration);
        Assert.Equal("ConnectionString", infra.OutputConfiguration[0].Key);
        Assert.Equal("Server=localhost", infra.OutputConfiguration[0].Value);
    }

    [Fact]
    public async Task Infrastructure_AddEntry_Multiple_Should_AllAppearInOutputConfiguration()
    {
        var infra = new MultiEntryInfrastructure();

        await infra.Initialize();

        Assert.Equal(2, infra.OutputConfiguration.Count);
    }

    private class MultiEntryInfrastructure : Infrastructure<string>
    {
        public MultiEntryInfrastructure() : base(Guid.NewGuid()) { }

        public override Task Initialize()
        {
            AddEntry("Key1", "Value1");
            AddEntry("Key2", "Value2");
            return Task.CompletedTask;
        }

        public override Task Reset() => Task.CompletedTask;
        public override Task Destroy() => Task.CompletedTask;
    }

    #endregion
}

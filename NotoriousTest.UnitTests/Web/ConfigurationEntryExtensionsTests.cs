using System.Globalization;

using AwesomeAssertions;

using Microsoft.Extensions.Configuration;

using NotoriousTest.Core.Configuration;
using NotoriousTest.Web.Helpers;

namespace NotoriousTest.UnitTests.Web;

public class ConfigurationEntryExtensionsTests
{
    private class DatabaseSettings
    {
        public string? ConnectionString { get; set; }
        public int Timeout { get; set; }
        public double Ratio { get; set; }
        public RetrySettings? Retry { get; set; }
        public List<string> Hosts { get; set; } = new();
        public List<RetrySettings> Replicas { get; set; } = new();
        public Dictionary<string, string> Tags { get; set; } = new();
    }

    private class RetrySettings
    {
        public int Count { get; set; }
    }

    private class WithIndexer
    {
        public string Name { get; set; } = "name";
        public string this[int index] => index.ToString();
    }

    private static List<ConfigurationEntry<object>> Entries(params (string Key, object Value)[] entries)
        => entries.Select(e => new ConfigurationEntry<object>(e.Value, e.Key)).ToList();

    private static DatabaseSettings FullSettings() => new()
    {
        ConnectionString = "Server=localhost",
        Timeout = 30,
        Ratio = 1.5,
        Retry = new RetrySettings { Count = 3 },
        Hosts = ["host1", "host2"],
        Replicas = [new RetrySettings { Count = 1 }, new RetrySettings { Count = 2 }],
        Tags = new Dictionary<string, string> { ["env"] = "test" }
    };

    #region ToAppSettings

    [Fact]
    public void ToAppSettings_Should_Return_Empty_Dictionary_When_No_Entries()
    {
        var result = new List<ConfigurationEntry<object>>().ToAppSettings();

        result.Should().BeEmpty();
    }

    [Fact]
    public void ToAppSettings_Should_Use_Key_When_Value_Is_Scalar()
    {
        var result = Entries(("Name", "value"), ("Port", 5432), ("Enabled", true)).ToAppSettings();

        result.Should().BeEquivalentTo(new Dictionary<string, string?>
        {
            ["Name"] = "value",
            ["Port"] = "5432",
            ["Enabled"] = "True"
        });
    }

    [Fact]
    public void ToAppSettings_Should_Map_Null_Property_To_Null_Value()
    {
        var result = Entries(("Database", new DatabaseSettings())).ToAppSettings();

        result.Should().Contain("Database:ConnectionString", null);
    }

    [Fact]
    public void ToAppSettings_Should_Flatten_Nested_Objects_With_Colon()
    {
        var result = Entries(("Database", FullSettings())).ToAppSettings();

        result.Should().Contain("Database:ConnectionString", "Server=localhost");
        result.Should().Contain("Database:Timeout", "30");
        result.Should().Contain("Database:Retry:Count", "3");
    }

    [Fact]
    public void ToAppSettings_Should_Index_Collections_With_Colon()
    {
        var result = Entries(("Database", FullSettings())).ToAppSettings();

        result.Should().Contain("Database:Hosts:0", "host1");
        result.Should().Contain("Database:Hosts:1", "host2");
        result.Should().Contain("Database:Replicas:0:Count", "1");
        result.Should().Contain("Database:Replicas:1:Count", "2");
    }

    [Fact]
    public void ToAppSettings_Should_Use_Dictionary_Keys_As_Sections()
    {
        var result = Entries(("Database", FullSettings())).ToAppSettings();

        result.Should().Contain("Database:Tags:env", "test");
    }

    [Fact]
    public void ToAppSettings_Should_Flatten_Root_Dictionary_Under_Entry_Key()
    {
        var result = Entries(("Section", new Dictionary<string, string> { ["A"] = "1", ["B"] = "2" })).ToAppSettings();

        result.Should().BeEquivalentTo(new Dictionary<string, string?>
        {
            ["Section:A"] = "1",
            ["Section:B"] = "2"
        });
    }

    [Fact]
    public void ToAppSettings_Should_Merge_All_Entries()
    {
        var result = Entries(("First", "1"), ("Second", new RetrySettings { Count = 2 })).ToAppSettings();

        result.Should().BeEquivalentTo(new Dictionary<string, string?>
        {
            ["First"] = "1",
            ["Second:Count"] = "2"
        });
    }

    [Fact]
    public void ToAppSettings_Should_Keep_Last_Value_When_Keys_Are_Duplicated()
    {
        var result = Entries(("Key", "first"), ("Key", "second")).ToAppSettings();

        result.Should().ContainSingle().Which.Should().Be(new KeyValuePair<string, string?>("Key", "second"));
    }

    [Fact]
    public void ToAppSettings_Should_Ignore_Indexers()
    {
        var result = Entries(("Section", new WithIndexer())).ToAppSettings();

        result.Should().BeEquivalentTo(new Dictionary<string, string?> { ["Section:Name"] = "name" });
    }

    [Fact]
    public void ToAppSettings_Should_Format_Values_With_Invariant_Culture()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

            var result = Entries(("Database", FullSettings())).ToAppSettings();

            result.Should().Contain("Database:Ratio", "1.5");
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void ToAppSettings_Should_Be_Bindable_By_IConfiguration()
    {
        DatabaseSettings expected = FullSettings();

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(Entries(("Database", expected)).ToAppSettings())
            .Build();

        configuration.GetSection("Database").Get<DatabaseSettings>().Should().BeEquivalentTo(expected);
    }

    #endregion

    #region ToEnvironmentVariables

    [Fact]
    public void ToEnvironmentVariables_Should_Return_Empty_Dictionary_When_No_Entries()
    {
        var result = new List<ConfigurationEntry<object>>().ToEnvironmentVariables();

        result.Should().BeEmpty();
    }

    [Fact]
    public void ToEnvironmentVariables_Should_Use_Key_When_Value_Is_Scalar()
    {
        var result = Entries(("Name", "value")).ToEnvironmentVariables();

        result.Should().BeEquivalentTo(new Dictionary<string, string?> { ["Name"] = "value" });
    }

    [Fact]
    public void ToEnvironmentVariables_Should_Flatten_Nested_Objects_With_Double_Underscore()
    {
        var result = Entries(("Database", FullSettings())).ToEnvironmentVariables();

        result.Should().Contain("Database__ConnectionString", "Server=localhost");
        result.Should().Contain("Database__Retry__Count", "3");
        result.Keys.Should().NotContain(key => key.Contains(':'));
    }

    [Fact]
    public void ToEnvironmentVariables_Should_Index_Collections_With_Double_Underscore()
    {
        var result = Entries(("Database", FullSettings())).ToEnvironmentVariables();

        result.Should().Contain("Database__Hosts__0", "host1");
        result.Should().Contain("Database__Hosts__1", "host2");
        result.Should().Contain("Database__Replicas__1__Count", "2");
    }

    [Fact]
    public void ToEnvironmentVariables_Should_Use_Dictionary_Keys_As_Sections()
    {
        var result = Entries(("Database", FullSettings())).ToEnvironmentVariables();

        result.Should().Contain("Database__Tags__env", "test");
    }

    [Fact]
    public void ToEnvironmentVariables_Should_Be_Bindable_By_IConfiguration()
    {
        DatabaseSettings expected = FullSettings();
        string prefix = $"NT_TEST_{Guid.NewGuid():N}_";
        Dictionary<string, string?> variables = Entries(("Database", expected)).ToEnvironmentVariables();

        try
        {
            foreach ((string key, string? value) in variables)
                Environment.SetEnvironmentVariable(prefix + key, value);

            IConfiguration configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables(prefix)
                .Build();

            configuration.GetSection("Database").Get<DatabaseSettings>().Should().BeEquivalentTo(expected);
        }
        finally
        {
            foreach (string key in variables.Keys)
                Environment.SetEnvironmentVariable(prefix + key, null);
        }
    }

    #endregion
}

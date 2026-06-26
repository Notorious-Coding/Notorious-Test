using Microsoft.Extensions.Configuration;
using NotoriousTest.Core.Settings;

namespace NotoriousTest.Internal.TestSettings;

internal class TestSettingsProvider : ITestSettingsProvider
{
    private IConfiguration _cache;

    public IConfiguration Find()
    {
        if (_cache == null)
        {
            string? settingsPath = FindFile("testsettings.json");


            var configurationBuilder = new ConfigurationBuilder();
            if (settingsPath != null) configurationBuilder.AddJsonFile(settingsPath);

            _cache = configurationBuilder.Build();
        }

        return _cache;
    }

    public T? Get<T>(string key) where T : new()
    {
        IConfigurationSection section = Find().GetSection(key);
        if (!section.Exists())
            return default;

        var config = new T();
        section.Bind(config);

        return config;
    }

    private static string? FindFile(string fileName) =>
        Directory
            .EnumerateFiles(AppContext.BaseDirectory, fileName, SearchOption.AllDirectories)
            .FirstOrDefault();
}

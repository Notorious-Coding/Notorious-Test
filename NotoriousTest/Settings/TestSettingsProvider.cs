using Microsoft.Extensions.Configuration;

namespace NotoriousTest.Settings
{
    internal class TestSettingsProvider : ITestSettingsProvider
    {
        private IConfiguration? _cache;
        public IConfiguration Find()
        {
            if (_cache == null)
            {
                string? settingsPath = FindFile("testsettings.json");

                if (settingsPath == null)
                    throw new FileNotFoundException("testsettings.json not found. Make sure the file exist and that CopyToOutputDirectory property is to PreserveNewest.");

                _cache = new ConfigurationBuilder()
                .AddJsonFile(settingsPath)
                .Build();
            }

            return _cache;
        }
        private static string? FindFile(string fileName)
        {
            return Directory
                .EnumerateFiles(AppContext.BaseDirectory, fileName, SearchOption.AllDirectories)
                .FirstOrDefault();
        }
    }
}

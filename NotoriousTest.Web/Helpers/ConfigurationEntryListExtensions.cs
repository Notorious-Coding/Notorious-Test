using NotoriousTest.Core.Configuration;

namespace NotoriousTest.Web;

public static class ConfigurationEntryListExtensions
{
    extension(List<ConfigurationEntry<object>> configuration)
    {
        public string? WebAppUrl
        {
            get => configuration.FirstOrDefault(x => x.Key == "WebAppUrl")?.Value?.ToString() ?? null;
            set => configuration.Add(new ConfigurationEntry<object>(value, "WebAppUrl"));
        }
    }
}

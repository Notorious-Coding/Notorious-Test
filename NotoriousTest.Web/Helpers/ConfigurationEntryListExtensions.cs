using NotoriousTest.Core.Configuration;

namespace NotoriousTest.Web;

public static class ConfigurationEntryListExtensions
{
    public const string WEB_APP_URL_CONFIGURATION_KEY = "WebAppUrl";
    public const string WEB_API_URL_CONFIGURATION_KEY = "WebApiUrl";

    extension(List<ConfigurationEntry<object>> configuration)
    {
        /// <summary>
        ///     Url for your web application.
        /// </summary>
        public string? WebAppUrl
        {
            get => configuration.FirstOrDefault(x => x.Key == WEB_APP_URL_CONFIGURATION_KEY)?.Value?.ToString() ?? null;
            set => configuration.Add(new ConfigurationEntry<object>(value, WEB_APP_URL_CONFIGURATION_KEY));
        }

        /// <summary>
        ///     Url for your web API, published by an ASP.NET Core application listening with Kestrel.
        /// </summary>
        public string? WebApiUrl
        {
            get => configuration.FirstOrDefault(x => x.Key == WEB_API_URL_CONFIGURATION_KEY)?.Value?.ToString() ?? null;
            set => configuration.Add(new ConfigurationEntry<object>(value, WEB_API_URL_CONFIGURATION_KEY));
        }
    }
}

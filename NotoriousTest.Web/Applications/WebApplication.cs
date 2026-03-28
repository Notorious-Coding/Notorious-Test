using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

using NotoriousTest.Core.Configuration;
using NotoriousTest.Web.Helpers;

namespace NotoriousTest.Web.Applications
{
    public class WebApplication<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IWebApplication where TEntryPoint : class
    {
        public List<ConfigurationEntry<object>> ConsumedConfiguration { get; set; } = new List<ConfigurationEntry<object>>();

        public virtual HttpClient CreateDefaultClient()
        {
            return base.CreateDefaultClient();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.ConfigureAppConfiguration((config) =>
            {
                Dictionary<string, string> aggregatedConfiguration = ConsumedConfiguration
                                                .Select(ce => ce.Value.ToDictionary(ce.Key))
                                                .SelectMany(d => d)
                                                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                config.AddInMemoryCollection(aggregatedConfiguration);
            });
        }


    }
}

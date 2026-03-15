using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

using NotoriousTest.Configuration;
using NotoriousTest.Web.Helpers;

namespace NotoriousTest.Web.Applications
{
    public abstract class WebApplication<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IConfigurationConsumer where TEntryPoint : class
    {
        public List<ConfigurationEntry<object>> ConsumedConfiguration { get; set; } = new List<ConfigurationEntry<object>>();

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

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

using NotoriousTest.Configuration;
using NotoriousTest.Web.Helpers;

namespace NotoriousTest.Web.Applications
{
    /// <summary>
    /// A <see cref="WebApplicationFactory{TEntryPoint}"/> that injects consumed configuration into the test web host.
    /// </summary>
    /// <typeparam name="TEntryPoint">The entry-point type of the application under test.</typeparam>
    public class WebApplication<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IWebApplication where TEntryPoint : class
    {
        /// <inheritdoc/>
        public List<ConfigurationEntry<object>> ConsumedConfiguration { get; set; } = new List<ConfigurationEntry<object>>();

        /// <inheritdoc/>
        public virtual HttpClient CreateDefaultClient()
        {
            return base.CreateDefaultClient();
        }

        /// <inheritdoc/>
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.ConfigureAppConfiguration((config) =>
            {
                Dictionary<string, string?> aggregatedConfiguration = ConsumedConfiguration
                                                .Select(ce => ce.Value.ToDictionary(ce.Key))
                                                .SelectMany(d => d)
                                                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                config.AddInMemoryCollection(aggregatedConfiguration);
            });
        }


    }
}

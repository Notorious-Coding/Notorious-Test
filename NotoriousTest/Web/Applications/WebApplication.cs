using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using NotoriousTest.Common.Configuration;

namespace NotoriousTest.Web.Applications
{
    public abstract class WebApplication<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IConfigurable where TEntryPoint : class
    {
        public Dictionary<string, string>? Configuration { get; set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.ConfigureAppConfiguration((config) =>
            {
                config.AddInMemoryCollection(Configuration);
            });
        }
    }
}

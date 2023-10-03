using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NotoriousTest.Web
{
    public abstract class ConfiguredWebApplication<TEntryPoint> : WebApplicationFactory<TEntryPoint> where TEntryPoint : class
    {
        protected Dictionary<string, string>? Configuration{ get; set; }

        public ConfiguredWebApplication<TEntryPoint> Configure(Dictionary<string, string>? configuration)
        {
            Configuration = configuration;
            return this;
        }

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

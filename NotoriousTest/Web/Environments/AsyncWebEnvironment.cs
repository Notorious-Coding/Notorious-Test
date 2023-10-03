using Microsoft.AspNetCore.Mvc.Testing;
using NotoriousTest.Common.Environments;
using NotoriousTest.Web.Infrastructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotoriousTest.Web.Environments
{
    public abstract class AsyncWebEnvironment<TEntryPoint> : AsyncEnvironment
        where TEntryPoint : class
    {
        public Task AddWebApplication(WebApplicationFactory<TEntryPoint> webApp)
        {
            ArgumentNullException.ThrowIfNull(webApp, nameof(webApp));            
            Infrastructures.Add(new WebApplicationAsyncInfrastructure<TEntryPoint>(webApp));

            return Task.CompletedTask;
        }

        public Task AddWebApplication(ConfiguredWebApplication<TEntryPoint> webApp)
        {
            ArgumentNullException.ThrowIfNull(webApp, nameof(webApp));
            webApp.Configure(Configuration);
            Infrastructures.Add(new WebApplicationAsyncInfrastructure<TEntryPoint>(webApp));

            return Task.CompletedTask;
        }

        public Task<WebApplicationAsyncInfrastructure<TEntryPoint>> GetWebApplication()
        {
            return GetInfrastructureAsync<WebApplicationAsyncInfrastructure<TEntryPoint>>();
        }
    }
}

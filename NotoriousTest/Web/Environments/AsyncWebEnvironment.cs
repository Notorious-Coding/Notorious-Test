using Microsoft.AspNetCore.Mvc.Testing;
using NotoriousTest.Common.Environments;
using NotoriousTest.Common.Helpers;
using NotoriousTest.Web.Applications;
using NotoriousTest.Web.Infrastructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotoriousTest.Web.Environments
{
    public abstract class AsyncWebEnvironment<TEntryPoint> : AsyncWebEnvironment<TEntryPoint, Dictionary<string, string>>
        where TEntryPoint : class
    {

    }

    public abstract class AsyncWebEnvironment<TEntryPoint, TConfig> : AsyncConfiguredEnvironment<TConfig>
        where TEntryPoint : class
        where TConfig : class, new()
    {
        public Task AddWebApplication(WebApplicationFactory<TEntryPoint> webApp)
        {
            ArgumentNullException.ThrowIfNull(webApp, nameof(webApp));
            AddInfrastructure(new WebApplicationAsyncInfrastructure<TEntryPoint, TConfig>(webApp));

            return Task.CompletedTask;
        }

        public Task AddWebApplication(ConfiguredWebApplication<TEntryPoint> webApp)
        {
            ArgumentNullException.ThrowIfNull(webApp, nameof(webApp));
            AddInfrastructure(new WebApplicationAsyncInfrastructure<TEntryPoint, TConfig>(webApp));
            return Task.CompletedTask;
        }

        public Task<WebApplicationAsyncInfrastructure<TEntryPoint, TConfig>> GetWebApplication()
        {
            return GetInfrastructureAsync<WebApplicationAsyncInfrastructure<TEntryPoint, TConfig>>();
        }
    }
}

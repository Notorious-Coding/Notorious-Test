using Microsoft.AspNetCore.Mvc.Testing;
using NotoriousTest.Common.Environments;
using NotoriousTest.Web.Applications;
using NotoriousTest.Web.Infrastructures;

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
            AddInfrastructure(new AsyncWebApplicationInfrastructure<TEntryPoint, TConfig>(webApp));

            return Task.CompletedTask;
        }

        public Task AddWebApplication(WebApplication<TEntryPoint> webApp)
        {
            ArgumentNullException.ThrowIfNull(webApp, nameof(webApp));
            AddInfrastructure(new AsyncWebApplicationInfrastructure<TEntryPoint, TConfig>(webApp));
            return Task.CompletedTask;
        }

        public Task<AsyncWebApplicationInfrastructure<TEntryPoint, TConfig>> GetWebApplication()
        {
            return GetInfrastructureAsync<AsyncWebApplicationInfrastructure<TEntryPoint, TConfig>>();
        }
    }
}

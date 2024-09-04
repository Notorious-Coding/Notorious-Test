using Microsoft.AspNetCore.Mvc.Testing;
using NotoriousTest.Common.Environments;
using NotoriousTest.Web.Applications;
using NotoriousTest.Web.Infrastructures;

namespace NotoriousTest.Web.Environments
{
    public abstract class WebEnvironment<TEntryPoint> : WebEnvironment<TEntryPoint, Dictionary<string, string>>
        where TEntryPoint : class
    {

    }

    public abstract class WebEnvironment<TEntryPoint, TConfig> : ConfiguredEnvironment<TConfig>
        where TEntryPoint : class
        where TConfig : class, new()
    {
        public void AddWebApplication(WebApplicationFactory<TEntryPoint> webApp)
        {
            ArgumentNullException.ThrowIfNull(webApp, nameof(webApp));
            AddInfrastructure(new WebApplicationInfrastructure<TEntryPoint, TConfig>(webApp));
        }

        public void AddWebApplication(WebApplication<TEntryPoint> webApp)
        {
            ArgumentNullException.ThrowIfNull(webApp, nameof(webApp));
            AddInfrastructure(new WebApplicationInfrastructure<TEntryPoint, TConfig>(webApp));
        }

        public WebApplicationInfrastructure<TEntryPoint, TConfig> GetWebApplication()
        {
            return GetInfrastructure<WebApplicationInfrastructure<TEntryPoint, TConfig>>();
        }
    }
}

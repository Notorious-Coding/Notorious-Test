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

        public void AddWebApplication(ConfiguredWebApplication<TEntryPoint> webApp)
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

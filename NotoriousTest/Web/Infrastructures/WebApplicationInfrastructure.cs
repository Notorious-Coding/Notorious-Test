using Microsoft.AspNetCore.Mvc.Testing;
using NotoriousTest.Common.Helpers;
using NotoriousTest.Common.Infrastructures.Async;
using NotoriousTest.Common.Infrastructures.Sync;
using NotoriousTest.Web.Applications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotoriousTest.Web.Infrastructures
{

    public class WebApplicationInfrastructure<TEntryPoint> : WebApplicationInfrastructure<TEntryPoint, Dictionary<string, string>>
    where TEntryPoint : class
    {

    }

    public class WebApplicationInfrastructure<TEntryPoint, TConfig> : ConfigurationConsumerInfrastructure<TConfig> where TEntryPoint : class
    {
        private WebApplicationFactory<TEntryPoint> _webApplicationFactory;
        public HttpClient? HttpClient;
        public override int Order => 999;

        public WebApplicationInfrastructure(WebApplicationFactory<TEntryPoint> webApplicationFactory)
        {
            _webApplicationFactory = webApplicationFactory;
        }

        public WebApplicationInfrastructure()
        {
            _webApplicationFactory = new WebApplicationFactory<TEntryPoint>();
        }


        public override void Destroy()
        {
            _webApplicationFactory.Dispose();
        }

        public override void Initialize()
        {
            if (_webApplicationFactory is IConfigurableApplication configurableApplication)
            {
                configurableApplication.Configuration = Configuration.ToDictionary();
            }

            HttpClient = _webApplicationFactory.CreateDefaultClient();
        }

        public override void Reset()
        {
        }
    }
}

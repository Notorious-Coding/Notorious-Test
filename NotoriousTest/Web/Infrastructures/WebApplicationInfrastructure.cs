using Microsoft.AspNetCore.Mvc.Testing;
using NotoriousTest.Common.Helpers;
using NotoriousTest.Common.Infrastructures.Sync;
using NotoriousTest.Web.Applications;

namespace NotoriousTest.Web.Infrastructures
{
    public class WebApplicationInfrastructure<TEntryPoint> : WebApplicationInfrastructure<TEntryPoint, Dictionary<string, string>>
    where TEntryPoint : class
    {

    }

    public class WebApplicationInfrastructure<TEntryPoint, TConfig> : ConfiguredInfrastructure<TConfig> 
        where TEntryPoint : class
        where TConfig : new()
    {
        private WebApplicationFactory<TEntryPoint> _webApplicationFactory;
        public HttpClient? HttpClient;
        public override int Order => 999;

        public WebApplicationInfrastructure(WebApplicationFactory<TEntryPoint> webApplicationFactory) : base()
        {
            _webApplicationFactory = webApplicationFactory;
        }

        public WebApplicationInfrastructure() : base()
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

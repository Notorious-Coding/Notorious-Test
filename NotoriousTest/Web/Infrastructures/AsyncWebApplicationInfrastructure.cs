using Microsoft.AspNetCore.Mvc.Testing;
using NotoriousTest.Common.Helpers;
using NotoriousTest.Common.Infrastructures;
using NotoriousTest.Common.Infrastructures.Async;
using NotoriousTest.Web.Applications;

namespace NotoriousTest.Web.Infrastructures
{
    public class WebApplicationAsyncInfrastructure<TEntryPoint> : AsyncWebApplicationInfrastructure<TEntryPoint, Dictionary<string, string>> 
        where TEntryPoint : class
    {

    }
    public class AsyncWebApplicationInfrastructure<TEntryPoint, TConfig> : AsyncConfiguredInfrastructure<TConfig>, IConfigurationConsumer
        where TEntryPoint : class
        where TConfig : class, new()
    {
        private WebApplicationFactory<TEntryPoint> _webApplicationFactory;
        public HttpClient? HttpClient;
        public override int Order => 999;

        public AsyncWebApplicationInfrastructure(WebApplicationFactory<TEntryPoint> webApplicationFactory)
        {
            _webApplicationFactory = webApplicationFactory;
        }

        public AsyncWebApplicationInfrastructure()
        {
            _webApplicationFactory = new WebApplicationFactory<TEntryPoint>();
        }

        public override async Task Destroy()
        {
            await _webApplicationFactory.DisposeAsync();
        }

        public override Task Initialize()
        {
            if(_webApplicationFactory is IConfigurableApplication configurableApplication)
            {
                configurableApplication.Configuration = Configuration.ToDictionary();
            }

            HttpClient = _webApplicationFactory.CreateDefaultClient();
            return Task.CompletedTask;
        }

        public override Task Reset()
        {
            return Task.CompletedTask;
        }
    }
}

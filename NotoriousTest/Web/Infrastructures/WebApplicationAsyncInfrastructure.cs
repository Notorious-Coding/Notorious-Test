using Microsoft.AspNetCore.Mvc.Testing;
using NotoriousTest.Common.Infrastructures.Async;

namespace NotoriousTest.Web.Infrastructures
{
    public class WebApplicationAsyncInfrastructure<TEntryPoint> : AsyncInfrastructure where TEntryPoint : class
    {
        private WebApplicationFactory<TEntryPoint> _webApplicationFactory;
        public HttpClient? HttpClient;
        public override int Order => 999;

        public WebApplicationAsyncInfrastructure(WebApplicationFactory<TEntryPoint> webApplicationFactory)
        {
            _webApplicationFactory = webApplicationFactory;
        }

        public WebApplicationAsyncInfrastructure()
        {
            _webApplicationFactory = new WebApplicationFactory<TEntryPoint>();
        }


        public override async Task Destroy()
        {
            await _webApplicationFactory.DisposeAsync();
        }

        public override Task Initialize()
        {
            HttpClient = _webApplicationFactory.CreateDefaultClient();
            return Task.CompletedTask;
        }

        public override Task Reset()
        {
            return Task.CompletedTask;
        }
    }
}

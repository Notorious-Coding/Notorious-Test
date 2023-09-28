using Microsoft.AspNetCore.Mvc.Testing;

namespace NotoriousTest.Infrastructures
{
    public abstract class WebApplicationAsyncInfrastructure<TEntryPoint> : AsyncInfrastructure where TEntryPoint : class
    {
        private WebApplicationFactory<TEntryPoint> _webApplicationFactory;
        public HttpClient HttpClient;
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

        public override async Task Initialize()
        {
            HttpClient = _webApplicationFactory.CreateDefaultClient();
        }

        public override async Task Reset()
        {
        }
    }
}

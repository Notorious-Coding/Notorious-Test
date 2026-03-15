using Microsoft.AspNetCore.Mvc.Testing;

using NotoriousTest.Configuration;
using NotoriousTest.Infrastructures;
namespace NotoriousTest.Web.Infrastructures
{
    public class WebApplicationInfrastructure<TEntryPoint> : Infrastructure, IConfigurationConsumer
        where TEntryPoint : class
    {
        private WebApplicationFactory<TEntryPoint> _webApplicationFactory;
        public HttpClient? HttpClient;
        public override int? Order => 999;

        public List<ConfigurationEntry<object>> ConsumedConfiguration { get; set; }

        public WebApplicationInfrastructure(WebApplicationFactory<TEntryPoint> webApplicationFactory) : base()
        {
            _webApplicationFactory = webApplicationFactory;
        }

        public WebApplicationInfrastructure() : base()
        {
            _webApplicationFactory = new WebApplicationFactory<TEntryPoint>();
        }

        public override async Task Destroy()
        {
            await _webApplicationFactory.DisposeAsync();
        }

        public override Task Initialize()
        {
            if (_webApplicationFactory is IConfigurationConsumer configurableApplication)
            {
                configurableApplication.ConsumedConfiguration = ConsumedConfiguration;
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

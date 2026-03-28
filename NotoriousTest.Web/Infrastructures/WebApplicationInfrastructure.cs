using NotoriousTest.Core;
using NotoriousTest.Core.Configuration;
using NotoriousTest.Core.Infrastructures;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Web.Applications;
namespace NotoriousTest.Web.Infrastructures
{
    public abstract class WebApplicationInfrastructure : Infrastructure, IConfigurationConsumer
    {
        public List<ConfigurationEntry<object>> ConsumedConfiguration { get; set; }
        public HttpClient? HttpClient;
        public override int? Order => 999;

        protected WebApplicationInfrastructure(ContextId contextId, ITestLogger logger, IRegistry registry) : base(contextId, logger, registry)
        {

        }

    }
    public class WebApplicationInfrastructure<TWebApp> : WebApplicationInfrastructure where TWebApp : IWebApplication, new()
    {
        private TWebApp _webApplicationFactory;
        public override int? Order => 999;

        public WebApplicationInfrastructure(ContextId contextId, ITestLogger logger, IRegistry registry) : base(contextId, logger, registry)
        {
            _webApplicationFactory = new TWebApp();
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

using Microsoft.AspNetCore.Mvc.Testing;

using NotoriousTest.Configuration;
using NotoriousTest.Helpers;
using NotoriousTest.Infrastructures;
namespace NotoriousTest.Web.Infrastructures
{
    public class WebApplicationInfrastructure<TEntryPoint> : WebApplicationInfrastructure<TEntryPoint, Dictionary<string, string>>
        where TEntryPoint : class
    {

    }
    public class WebApplicationInfrastructure<TEntryPoint, TConfig> : Infrastructure, IConfigurable<TConfig>
        where TEntryPoint : class
        where TConfig : class, new()
    {
        private WebApplicationFactory<TEntryPoint> _webApplicationFactory;
        public HttpClient? HttpClient;
        public override int? Order => 999;

        public TConfig Configuration { get; set; } = new();

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
            if (_webApplicationFactory is IConfigurable configurableApplication)
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

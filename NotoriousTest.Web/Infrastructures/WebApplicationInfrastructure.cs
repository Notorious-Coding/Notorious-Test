using NotoriousTest.Core;
using NotoriousTest.Core.Configuration;
using NotoriousTest.Core.Infrastructures;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Web.Applications;

namespace NotoriousTest.Web.Infrastructures;

public abstract class WebApplicationInfrastructure : Infrastructure, IConfigurationConsumer
{
    public HttpClient? HttpClient;

    protected WebApplicationInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry) : base(
        contextId, logger, registry)
    {
    }

    public override int? Order => 999;
    public override bool DisableRegistry => true;
    public List<ConfigurationEntry<object>> ConsumedConfiguration { get; set; }
}

public class WebApplicationInfrastructure<TWebApp> : WebApplicationInfrastructure where TWebApp : IWebApplication, new()
{
    private TWebApp _webApplicationFactory;

    public WebApplicationInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry) : base(
        contextId, logger, registry)
    {
        _webApplicationFactory = new TWebApp();
        Dependencies = _webApplicationFactory.Dependencies;
        Requirements = _webApplicationFactory.Requirements;
    }

    public override int? Order => 999;

    public override async Task Destroy() => await _webApplicationFactory.DisposeAsync();

    public override async Task Initialize()
    {
        if (_webApplicationFactory is IConfigurationConsumer configurableApplication)
            configurableApplication.ConsumedConfiguration = ConsumedConfiguration;

        HttpClient = await _webApplicationFactory.Start();
    }
}

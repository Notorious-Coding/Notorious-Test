using NotoriousTest.Core;
using NotoriousTest.Core.Configuration;
using NotoriousTest.Core.Infrastructures;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;

namespace NotoriousTest.Web.Infrastructures;

public abstract class WebApplicationInfrastructure : Infrastructure, IConfigurationConsumer
{
    /// <summary>
    ///     HttpClient to interact with the application.
    /// </summary>
    public HttpClient? HttpClient;

    protected WebApplicationInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry) : base(
        contextId, logger, registry)
    {
    }

    ///<inheritdoc />
    public override int? Order => 999;

    ///<inheritdoc />
    public override bool DisableRegistry => true;

    /// <summary>
    ///     Static configuration to be passed to the application.
    /// </summary>
    public Dictionary<string, string> AdditionalConfiguration { get; set; } = new();

    public List<ConfigurationEntry<object>> ConsumedConfiguration { get; set; } = new();
}

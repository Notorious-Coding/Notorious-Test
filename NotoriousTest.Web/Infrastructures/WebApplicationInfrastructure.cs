using NotoriousTest.Configuration;
using NotoriousTest.Infrastructures;
using NotoriousTest.Logger;
using NotoriousTest.Web.Applications;
namespace NotoriousTest.Web.Infrastructures
{
    /// <summary>
    /// Infrastructure base class that hosts a web application factory for integration testing.
    /// </summary>
    public abstract class WebApplicationInfrastructure : Infrastructure, IConfigurationConsumer
    {
        /// <inheritdoc/>
        public List<ConfigurationEntry<object>> ConsumedConfiguration { get; set; } = [];

        /// <summary>Gets or sets the <see cref="HttpClient"/> created from the web application factory.</summary>
        public HttpClient? HttpClient;

        /// <inheritdoc/>
        public override int? Order => 999;

        /// <summary>Initializes a new instance with the given context and logger.</summary>
        /// <param name="contextId">The environment context identifier.</param>
        /// <param name="logger">The test logger.</param>
        protected WebApplicationInfrastructure(ContextId contextId, ITestLogger logger) : base(contextId, logger)
        {

        }

    }

    /// <summary>
    /// Concrete infrastructure that creates and manages a typed web application factory.
    /// </summary>
    /// <typeparam name="TWebApp">The web application factory type.</typeparam>
    public class WebApplicationInfrastructure<TWebApp> : WebApplicationInfrastructure where TWebApp : IWebApplication, new()
    {
        private TWebApp _webApplicationFactory;

        /// <inheritdoc/>
        public override int? Order => 999;

        /// <summary>Initializes a new instance and creates the web application factory.</summary>
        /// <param name="contextId">The environment context identifier.</param>
        /// <param name="logger">The test logger.</param>
        public WebApplicationInfrastructure(ContextId contextId, ITestLogger logger) : base(contextId, logger)
        {
            _webApplicationFactory = new TWebApp();
        }

        /// <inheritdoc/>
        public override async Task Destroy()
        {
            await _webApplicationFactory.DisposeAsync();
        }

        /// <inheritdoc/>
        public override Task Initialize()
        {
            if (_webApplicationFactory is IConfigurationConsumer configurableApplication)
            {
                configurableApplication.ConsumedConfiguration = ConsumedConfiguration;
            }

            HttpClient = _webApplicationFactory.CreateDefaultClient();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override Task Reset()
        {
            return Task.CompletedTask;
        }
    }
}

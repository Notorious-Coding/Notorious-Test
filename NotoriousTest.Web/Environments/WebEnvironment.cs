using NotoriousTest.Web.Applications;
using NotoriousTest.Web.Infrastructures;

using Xunit.Sdk;

using Environment = NotoriousTest.Environments.Environment;

namespace NotoriousTest.Web.Environments
{
    /// <summary>
    /// Testing environment that supports a web application infrastructure.
    /// </summary>
    public abstract class WebEnvironment : Environment
    {
        /// <summary>Initializes a new instance with the given xUnit message sink.</summary>
        /// <param name="sink">The xUnit message sink for diagnostic output.</param>
        protected WebEnvironment(IMessageSink sink) : base(sink)
        {
        }

        /// <summary>Gets the registered <see cref="WebApplicationInfrastructure"/> from the environment.</summary>
        public WebApplicationInfrastructure WebApp => GetInfrastructure<WebApplicationInfrastructure>();

        /// <summary>
        /// Adds a web application factory to the current web environment configuration.
        /// </summary>
        /// <typeparam name="TWebApp">The web application factory type to register.</typeparam>
        /// <returns>The current web environment instance with the web application factory added.</returns>
        public WebEnvironment AddWebApplication<TWebApp>() where TWebApp : IWebApplication, new()
        {
            AddInfrastructure<WebApplicationInfrastructure<TWebApp>>();

            return this;
        }
    }
}

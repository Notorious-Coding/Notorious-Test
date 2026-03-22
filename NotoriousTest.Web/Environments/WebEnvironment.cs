using NotoriousTest.Web.Applications;
using NotoriousTest.Web.Infrastructures;

using Environment = NotoriousTest.Environments.Environment;

namespace NotoriousTest.Web.Environments
{
    /// <summary>
    /// Testing environment that support Web application.
    /// </summary>
    /// <typeparam name="TEntryPoint">An entrypoint to your program.cs</typeparam>
    public abstract class WebEnvironment : Environment
    {
        public WebApplicationInfrastructure WebApp => GetInfrastructure<WebApplicationInfrastructure>();
        /// <summary>
        /// Adds a web application factory to the current web environment configuration.
        /// </summary>
        /// <param name="webApp">The web application factory instance to be added. Cannot be null.</param>
        /// <returns>The current web environment instance with the web application factory added.</returns>
        public WebEnvironment AddWebApplication<TWebApp>() where TWebApp : IWebApplication, new()
        {
            AddInfrastructure<WebApplicationInfrastructure<TWebApp>>();

            return this;
        }
    }
}

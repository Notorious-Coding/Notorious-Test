using Microsoft.AspNetCore.Mvc.Testing;

using NotoriousTest.Web.Applications;
using NotoriousTest.Web.Infrastructures;

using Environment = NotoriousTest.Environments.Environment;

namespace NotoriousTest.Web.Environments
{
    /// <summary>
    /// Testing environment that support Web application.
    /// </summary>
    /// <typeparam name="TEntryPoint">An entrypoint to your program.cs</typeparam>
    public abstract class WebEnvironment<TEntryPoint> : Environment
        where TEntryPoint : class
    {
        /// <summary>
        /// Adds a web application factory to the current web environment configuration.
        /// </summary>
        /// <param name="webApp">The web application factory instance to be added. Cannot be null.</param>
        /// <returns>The current web environment instance with the web application factory added.</returns>
        public WebEnvironment<TEntryPoint> AddWebApplication(WebApplicationFactory<TEntryPoint> webApp)
        {
            ArgumentNullException.ThrowIfNull(webApp, nameof(webApp));
            AddInfrastructure(new WebApplicationInfrastructure<TEntryPoint>(webApp));

            return this;
        }

        /// <summary>
        /// Adds a web application to the current web environment configuration.
        /// </summary>
        /// <param name="webApp">The web application instance to add. Cannot be null.</param>
        /// <returns>The current web environment instance with the web application added.</returns>
        public WebEnvironment<TEntryPoint> AddWebApplication(WebApplication<TEntryPoint> webApp)
        {
            ArgumentNullException.ThrowIfNull(webApp, nameof(webApp));
            AddInfrastructure(new WebApplicationInfrastructure<TEntryPoint>(webApp));
            return this;
        }

        /// <summary>
        /// Retrieve environment web application.
        /// </summary>
        /// <returns>Web application.</returns>
        public WebApplicationInfrastructure<TEntryPoint> GetWebApplication()
        {
            return GetInfrastructure<WebApplicationInfrastructure<TEntryPoint>>();
        }
    }
}

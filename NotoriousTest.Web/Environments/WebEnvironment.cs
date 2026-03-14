using Microsoft.AspNetCore.Mvc.Testing;

using NotoriousTest.Environments;
using NotoriousTest.Web.Applications;
using NotoriousTest.Web.Infrastructures;

namespace NotoriousTest.Web.Environments
{
    public abstract class WebEnvironment<TEntryPoint> : WebEnvironment<TEntryPoint, Dictionary<string, string>>
        where TEntryPoint : class
    {

    }

    /// <summary>
    /// Testing environment that support Web application.
    /// </summary>
    /// <typeparam name="TEntryPoint">An entrypoint to your program.cs</typeparam>
    /// <typeparam name="TConfig">Configuration type </typeparam>
    public abstract class WebEnvironment<TEntryPoint, TConfig> : ConfiguredEnvironment<TConfig>
        where TEntryPoint : class
        where TConfig : class, new()
    {
        /// <summary>
        /// Adds a web application factory to the current web environment configuration.
        /// </summary>
        /// <param name="webApp">The web application factory instance to be added. Cannot be null.</param>
        /// <returns>The current web environment instance with the web application factory added.</returns>
        public WebEnvironment<TEntryPoint, TConfig> AddWebApplication(WebApplicationFactory<TEntryPoint> webApp)
        {
            ArgumentNullException.ThrowIfNull(webApp, nameof(webApp));
            AddInfrastructure(new WebApplicationInfrastructure<TEntryPoint, TConfig>(webApp));

            return this;
        }

        /// <summary>
        /// Adds a web application to the current web environment configuration.
        /// </summary>
        /// <param name="webApp">The web application instance to add. Cannot be null.</param>
        /// <returns>The current web environment instance with the web application added.</returns>
        public WebEnvironment<TEntryPoint, TConfig> AddWebApplication(WebApplication<TEntryPoint> webApp)
        {
            ArgumentNullException.ThrowIfNull(webApp, nameof(webApp));
            AddInfrastructure(new WebApplicationInfrastructure<TEntryPoint, TConfig>(webApp));
            return this;
        }

        /// <summary>
        /// Retrieve environment web application.
        /// </summary>
        /// <returns>Web application.</returns>
        public WebApplicationInfrastructure<TEntryPoint, TConfig> GetWebApplication()
        {
            return GetInfrastructure<WebApplicationInfrastructure<TEntryPoint, TConfig>>();
        }
    }
}

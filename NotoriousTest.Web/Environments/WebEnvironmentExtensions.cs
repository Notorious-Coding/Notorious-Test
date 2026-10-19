using NotoriousTest.Core.Environments;
using NotoriousTest.Web.Infrastructures;

namespace NotoriousTest.Web;

/// <summary>
///     Extensions for <see cref="EnvironmentBase" />.
/// </summary>
public static class WebEnvironmentExtensions
{
    /// <summary>
    ///     Get the first web application registered in the environment.
    /// </summary>
    /// <param name="environment">Environment.</param>
    /// <returns>The web application infrastructure.</returns>
    [Obsolete("Use WebApplication Extension props instead")]
    public static WebApplicationInfrastructure GetWebApplication(this EnvironmentBase environment) =>
        environment.GetInfrastructure<WebApplicationInfrastructure>();

    extension(EnvironmentBase environment)
    {
        /// <summary>
        ///     Get first web application registered in the environment.
        /// </summary>
        private WebApplicationInfrastructure WebApplication =>
            environment.GetInfrastructure<WebApplicationInfrastructure>();

        /// <summary>
        ///     Get the HttpClient instance for the first web application registered in the environment.
        /// </summary>
        /// <exception cref="InvalidOperationException">If no webapplication configured in the environment, throws</exception>
        /// .
        private HttpClient HttpClient => environment.WebApplication.HttpClient ??
                                         throw new InvalidOperationException(
                                             "No web application configured.");
    }
}

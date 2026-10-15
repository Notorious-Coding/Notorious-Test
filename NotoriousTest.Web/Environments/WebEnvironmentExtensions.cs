using NotoriousTest.Core.Environments;
using NotoriousTest.Web.Infrastructures;

namespace NotoriousTest.Web;

public static class WebEnvironmentExtensions
{
    /// <summary>
    ///     Get the first web application registered in the environment.
    /// </summary>
    /// <param name="environment">Environment.</param>
    /// <returns>The web application infrastructure.</returns>
    public static WebApplicationInfrastructure GetWebApplication(this EnvironmentBase environment) =>
        environment.GetInfrastructure<WebApplicationInfrastructure>();
}

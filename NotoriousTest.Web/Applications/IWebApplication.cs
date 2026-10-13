using NotoriousTest.Core.Configuration;
using NotoriousTest.Core.Infrastructures.Dependencies;

namespace NotoriousTest.Web.Applications;

/// <summary>
///     Create a web application accessible via <see cref="HttpClient" />.
/// </summary>
public interface IWebApplication : IAsyncDisposable, IConfigurationConsumer
{
    /// <summary>
    ///     WebApplication dependencies to install.
    /// </summary>
    List<IInfrastructureDependency> Dependencies { get; }

    /// <summary>
    ///     WebApplication requirements to meet.
    /// </summary>
    List<IInfrastructureRequirement> Requirements { get; }

    /// <summary>
    ///     Additional configuration to be passed to the application.
    /// </summary>
    Dictionary<string, string> AdditionalConfiguration { get; set; }

    /// <summary>
    ///     Start the application.
    /// </summary>
    /// <returns>A HttpClient configured to access the server.</returns>
    Task<HttpClient> Start();
}

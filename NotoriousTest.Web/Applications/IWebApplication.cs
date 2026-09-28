using NotoriousTest.Core.Configuration;
using NotoriousTest.Core.Infrastructures.Dependencies;

namespace NotoriousTest.Web.Applications;

/// <summary>
///     Create a web application accessible via <see cref="HttpClient" />.
/// </summary>
public interface IWebApplication : IAsyncDisposable, IConfigurationConsumer
{
    List<IInfrastructureDependency> Dependencies { get; }
    List<IInfrastructureRequirement> Requirements { get; }

    /// <summary>
    ///     Start the application.
    /// </summary>
    /// <returns>A HttpClient configured to access the server.</returns>
    Task<HttpClient> Start();
}

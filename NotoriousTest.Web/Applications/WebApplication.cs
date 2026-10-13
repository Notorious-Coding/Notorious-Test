using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using NotoriousTest.Core.Configuration;
using NotoriousTest.Core.Infrastructures.Dependencies;
using NotoriousTest.Web.Helpers;

namespace NotoriousTest.Web.Applications;

/// <summary>
///     Start a web application from <see cref="WebApplicationFactory{TEntryPoint}" />.
/// </summary>
/// <typeparam name="TEntryPoint">Type of the Program.cs file of the application to start.</typeparam>
public class WebApplication<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IWebApplication where TEntryPoint : class
{
    public List<ConfigurationEntry<object>> ConsumedConfiguration { get; set; } = new();
    public List<IInfrastructureDependency> Dependencies { get; } = [];
    public List<IInfrastructureRequirement> Requirements { get; } = [];

    /// <summary>
    ///     Additional configuration to be passed;
    /// </summary>
    public virtual Dictionary<string, string> AdditionalConfiguration { get; set; } = new();

    public virtual Task<HttpClient> Start() => Task.FromResult(CreateDefaultClient());

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(ConsumedConfiguration.ToAppSettings()!);
            config.AddInMemoryCollection(AdditionalConfiguration!);
        });
    }
}

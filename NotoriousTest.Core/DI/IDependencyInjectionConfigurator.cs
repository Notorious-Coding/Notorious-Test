using Microsoft.Extensions.DependencyInjection;

namespace NotoriousTest.Core.DI;

/// <summary>
///     Class responsible for configuring the DI container.
/// </summary>
public interface IDependencyInjectionConfigurator
{
    /// <summary>
    ///     Configure the DI container.
    /// </summary>
    /// <param name="services">DI Container.</param>
    /// <returns>A DI Container configured.</returns>
    IServiceCollection ConfigureServices(IServiceCollection services);
}

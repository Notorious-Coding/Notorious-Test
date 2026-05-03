using Microsoft.Extensions.DependencyInjection;

namespace NotoriousTest.Core.DI;

public interface IDependencyInjectionConfigurator
{
    IServiceCollection ConfigureServices(IServiceCollection services);
}

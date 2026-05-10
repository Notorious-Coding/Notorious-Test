using DoggyDog.UI.Context;
using DoggyDog.UI.Context.Adapters;
using DoggyDog.UI.Infrastructures.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DoggyDog.UI;

public static class DependencyInjection
{
    private static IConfiguration Configuration => field ??= new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true).Build();
    private static IServiceProvider Provider => field ??= Configure(new ServiceCollection(), Configuration).BuildServiceProvider();
    private static IServiceCollection Configure(IServiceCollection services, IConfiguration configuration) =>
        services
            .AddInfrastructures()
            .AddSingleton<EnvironmentContext>();

    public static async Task<T> Use<T>() where T : IContext
    {
        T instance = Provider.GetRequiredService<T>();
        await instance.OnMount();

        return instance;
    }
}

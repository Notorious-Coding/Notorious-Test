using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using NotoriousTest.Core;
using NotoriousTest.Core.DI;
using NotoriousTest.Core.Environments;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Runtime;
using NotoriousTest.Core.Settings;
using NotoriousTest.Core.Watchdog;
using NotoriousTest.Environments;
using NotoriousTest.Runtime;
using NotoriousTest.SqlLiteRegistry;
using NotoriousTest.Watchdog;

namespace NotoriousTest.DI;

public class DependencyInjectionConfigurator : IDependencyInjectionConfigurator
{
    public virtual IServiceCollection ConfigureServices(IServiceCollection services)
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = SettingsDefaults.DefaultRegistryConnectionString
        };

        var registryConfig = new SqliteRegistryProviderConfiguration()
        {
            ConnectionString = builder.ConnectionString
        };

        var settingsProvider = new TestSettingsProvider();
        services
            .AddSingleton<ITestSettingsProvider>(settingsProvider)
            .AddSingleton(settingsProvider.Get<EnvironmentSettings>(EnvironmentSettings.SECTION_NAME) ?? new EnvironmentSettings())
            .AddSingleton<IRegistry>((_) => new SqliteRegistryProvider(registryConfig))
            .AddSingleton<IWatchDog>((services) => new DoggyDogWatchDog(registryConfig, services.GetRequiredService<ITestLogger>(), settingsProvider.Get<DoggyDogWatchdogConfiguration>("Watchdog") ?? new DoggyDogWatchdogConfiguration()))
            .AddSingleton<IRuntime, RuntimeConfigurationProvider>();

        return services;
    }
}

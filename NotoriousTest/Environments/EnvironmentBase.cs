

using Microsoft.Extensions.DependencyInjection;

using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Settings;
using NotoriousTest.SqlLiteRegistry;

namespace NotoriousTest.Environments
{
    public abstract class EnvironmentBase : Core.Environments.EnvironmentBase
    {
        public override void ConfigureInfrastructureServices(IServiceCollection collection)
        {
            base.ConfigureInfrastructureServices(collection);

            collection.AddSingleton<ITestSettingsProvider, TestSettingsProvider>()
                .AddSingleton<IRegistry, SqliteRegistryProvider>();

        }
    }
}

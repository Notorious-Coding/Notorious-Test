using Microsoft.Extensions.DependencyInjection;

using NotoriousTest.Core.Logger;
using NotoriousTest.Environments;
using NotoriousTest.TUnit.Logger;

using TUnit.Core.Interfaces;

namespace NotoriousTest.TUnit
{
    public abstract class Environment : EnvironmentBase, IAsyncInitializer, IAsyncDisposable
    {
        protected override void ConfigureInfrastructureServices(IServiceCollection collection)
        {
            base.ConfigureInfrastructureServices(collection);

            collection
                .AddSingleton<ITestLogger, TestLogger>();
        }

        public async ValueTask DisposeAsync()
        {
            await Destroy();
        }


        async Task IAsyncInitializer.InitializeAsync()
        {
            await Initialize();
        }
    }
}

using Microsoft.Extensions.DependencyInjection;

using NotoriousTest.Core.Logger;
using NotoriousTest.Environments;
using NotoriousTest.NUnit.Logger;

using System.Threading.Tasks;

namespace NotoriousTest.NUnit
{
    public abstract class Environment : EnvironmentBase
    {
        public override void ConfigureInfrastructureServices(IServiceCollection collection)
        {
            base.ConfigureInfrastructureServices(collection);

            collection
                .AddSingleton<ITestLogger, TestLogger>();
        }

        public async Task InitializeAsync()
        {
            await Initialize();
        }

        public async Task DestroyAsync()
        {
            await Destroy();
        }
    }
}

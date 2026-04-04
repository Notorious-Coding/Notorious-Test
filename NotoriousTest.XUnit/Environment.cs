using Microsoft.Extensions.DependencyInjection;

using NotoriousTest.Core.Logger;
using NotoriousTest.Environments;
using NotoriousTest.XUnit.Logger;

using System.Threading.Tasks;

using Xunit;
using Xunit.Sdk;

namespace NotoriousTest.XUnit
{
    public abstract class Environment : EnvironmentBase, IAsyncLifetime
    {
        private readonly IMessageSink _sink;

        protected Environment(IMessageSink sink)
        {
            _sink = sink;
        }

        public override void ConfigureInfrastructureServices(IServiceCollection collection)
        {
            base.ConfigureInfrastructureServices(collection);

            collection
                .AddSingleton(_sink)
                .AddSingleton<ITestLogger, TestLogger>();
        }

        public async ValueTask DisposeAsync()
        {
            await Destroy();
        }

        public async ValueTask InitializeAsync()
        {
            await Initialize();
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NotoriousTest.Core.Logger;
using NotoriousTest.Environments;
using NotoriousTest.MSTest.Logger;

using System.Threading.Tasks;

namespace NotoriousTest.MSTest
{
    public abstract class Environment : EnvironmentBase
    {
        private readonly TestContext _testContext;

        protected Environment(TestContext testContext)
        {
            _testContext = testContext;
        }

        public override void ConfigureInfrastructureServices(IServiceCollection collection)
        {
            base.ConfigureInfrastructureServices(collection);

            collection
                .AddSingleton(_testContext)
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

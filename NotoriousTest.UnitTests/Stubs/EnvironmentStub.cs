using Microsoft.Extensions.DependencyInjection;

using NotoriousTest.Core.Environments;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Watchdog;

using System.Reflection;

namespace NotoriousTest.UnitTests.Stubs
{
    public class EnvironmentStub : EnvironmentBase
    {
        private readonly ITestLogger _logger;
        private readonly IWatchDog _watchDog;
        private readonly IRegistry _registry;

        public override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();
        public Func<Task>? OnConfigureEnvironment { get; set; }
        public Action<IServiceCollection>? OnConfigureInfrastructureServices { get; set; }

        public IServiceProvider PublicServiceProvider => ServiceProvider;


        public EnvironmentStub(ITestLogger logger, IWatchDog watchDog, IRegistry registry)
        {
            _logger = logger;
            _watchDog = watchDog;
            _registry = registry;
        }
        public override Task ConfigureEnvironment() => OnConfigureEnvironment?.Invoke() ?? Task.CompletedTask;

        public override void ConfigureInfrastructureServices(IServiceCollection collection)
        {
            base.ConfigureInfrastructureServices(collection);

            collection.AddSingleton(_logger);
            collection.AddSingleton(_registry);
            collection.AddSingleton(_watchDog);

            OnConfigureInfrastructureServices?.Invoke(collection);
        }
    }
}

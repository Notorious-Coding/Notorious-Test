using NotoriousTest.Core.Configuration;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;

namespace NotoriousTest.UnitTests.Stubs
{
    public class ConsumerInfrastructureStub : InfrastructureStub, IConfigurationConsumer
    {
        public ConsumerInfrastructureStub(ITestLogger logger, IRegistry provider, int? order = null, bool disableRegistry = false) : base(logger, provider, order, disableRegistry)
        {
        }

        public List<ConfigurationEntry<object>> ConsumedConfiguration { get; set; }
    }

    public class InfrastructureStub : NotoriousTest.Core.Infrastructures.Infrastructure<string, object>
    {
        private readonly int? _order;
        private readonly bool _disableRegistry;

        public override int? Order => _order;

        public override bool DisableRegistry => _disableRegistry;
        public Func<Task>? OnInitialize { get; set; }
        public Func<Task>? OnReset { get; set; }
        public Func<Task>? OnDestroy { get; set; }

        public InfrastructureStub(ITestLogger logger, IRegistry provider, int? order = null, bool disableRegistry = false, bool autoReset = true) : base(Guid.NewGuid(), logger, provider)
        {
            _order = order;
            _disableRegistry = disableRegistry;
            AutoReset = autoReset;
        }

        public override Task Destroy() => OnDestroy?.Invoke() ?? Task.CompletedTask;

        public override Task Initialize() => OnInitialize?.Invoke() ?? Task.CompletedTask;

        public override Task Reset() => OnReset?.Invoke() ?? Task.CompletedTask;
    }

    public class RegisteredInfrastructureStub : InfrastructureStub
    {
        public RegisteredInfrastructureStub(ITestLogger logger, IRegistry provider, int? order = null, bool disableRegistry = false) : base(logger, provider, order, disableRegistry)
        {
        }
        public override async Task Initialize()
        {
            await Register();
            await base.Initialize();
        }
    }
}

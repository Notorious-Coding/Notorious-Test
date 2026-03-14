using Xunit;

namespace NotoriousTest.Common.Infrastructures
{
    /// <summary>
    /// AsyncInfrastructure is a base class to define a test infrastructure.
    /// </summary>
    public abstract class Infrastructure : IInfrastructure, IAsyncLifetime, IAsyncDisposable
    {
        public virtual int? Order { get; }

        public bool AutoReset { get; set; } = true;

        public Guid ContextId { get; set; } = Guid.NewGuid();

        private bool _initialize = false;

        public Infrastructure(bool initialize = false)
        {
            _initialize = initialize;
        }

        public abstract Task Initialize();
        public abstract Task Reset();
        public abstract Task Destroy();

        /// <summary>
        /// Called by xunit
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_initialize) await Initialize();
        }

        public async Task DisposeAsync()
        {
            await Destroy();
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await DisposeAsync();
        }

        ~Infrastructure()
        {
            Destroy().Wait();
        }
    }
}

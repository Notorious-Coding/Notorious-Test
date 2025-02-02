using Xunit;

namespace NotoriousTest.Common.Infrastructures.Async
{
    public abstract class AsyncInfrastructure : IAsyncLifetime, IAsyncDisposable
    {
        public abstract int Order { get; }
        private bool _initialize = false;
        public bool AutoReset { get; private set; } = true;
        public AsyncInfrastructure(bool initialize = false, bool autoreset = true)
        {
            AutoReset = autoreset;
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

        ~AsyncInfrastructure()
        {
            Destroy().Wait();
        }
    }
}

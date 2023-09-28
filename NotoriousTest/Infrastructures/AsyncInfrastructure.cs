using Xunit;

namespace NotoriousTest.Infrastructures
{
    public abstract class AsyncInfrastructure : IAsyncLifetime, IAsyncDisposable
    {
        private bool _initialize = false;
        public AsyncInfrastructure(bool initialize = false)
        {
            _initialize = initialize;
        }

        public abstract int Order { get; }
        public abstract Task Initialize();
        public abstract Task Reset();
        public abstract Task Destroy();

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
    }
}

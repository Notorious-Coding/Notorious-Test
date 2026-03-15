using NotoriousTest.Configuration;

using Xunit;

namespace NotoriousTest.Infrastructures
{
    ///<inheritdoc/>
    public abstract class Infrastructure : Infrastructure<object>
    {
        public Infrastructure(bool initialize = false) : base(initialize)
        {

        }

    }
    /// <summary>
    /// AsyncInfrastructure is a base class to define a test infrastructure.
    /// </summary>
    public abstract class Infrastructure<TOutputConfiguration> : IConfigurationProducer<TOutputConfiguration>, IInfrastructure, IAsyncLifetime, IAsyncDisposable
    {
        ///<inheritdoc/>
        public virtual int? Order { get; }
        ///<inheritdoc/>
        public bool AutoReset { get; set; } = true;
        ///<inheritdoc/>
        public Guid ContextId { get; set; } = Guid.NewGuid();

        ///<innheritdoc/>
        public List<ConfigurationEntry<TOutputConfiguration>> OutputConfiguration { get; set; } = new();

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
        public async ValueTask InitializeAsync()
        {
            if (_initialize) await Initialize();
        }

        public async ValueTask DisposeAsync()
        {
            await Destroy();
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await DisposeAsync();
        }

        /// <summary>
        /// Add an output configuration.
        /// </summary>
        /// <param name="key">Key or path of the value within the configuration.</param>
        /// <param name="value">Value of the configuration.</param>
        public void AddOutputConfigurationEntry(string key, TOutputConfiguration value)
        {
            OutputConfiguration.Add(new ConfigurationEntry<TOutputConfiguration>(value, key));
        }

        /// <summary>
        /// Add an output configuration with default key to value type.
        /// </summary>
        /// <param name="value">Value of the configuration.</param>
        public void AddOutputConfigurationEntry(TOutputConfiguration value)
        {
            OutputConfiguration.Add(new ConfigurationEntry<TOutputConfiguration>(value, value!.GetType().Name));
        }
    }
}

namespace NotoriousTest.Configuration
{
    /// <summary>
    /// Make a class able to produce or consume configuration
    /// </summary>
    /// <typeparam name="T">Configuration type.</typeparam>
    public interface IConfigurableInfrastructure<TOutput> where TOutput : class
    {
        /// <summary>
        /// Gets or sets the configuration produced.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public TOutput OutputConfiguration { get; set; }
    }

    public interface IConfigurableInfrastructure<TOutput, TInput> : IConfigurableInfrastructure<TOutput> where TOutput : class where TInput : class
    {
        /// <summary>
        /// Gets or sets the configuration produced.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public TInput InputConfiguration { get; set; }
    }

    ///<inheritdoc/>
    public interface IDictionaryConfigurableInfrastructure : IConfigurableInfrastructure<Dictionary<string, string>>
    {
    }

    public interface IDictionaryConfigurableInfrastructure<TInput> : IConfigurableInfrastructure<Dictionary<string, string>, TInput> where TInput : class, new()
    {
    }
}


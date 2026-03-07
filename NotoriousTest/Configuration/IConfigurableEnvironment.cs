namespace NotoriousTest.Configuration
{
    /// <summary>
    /// Make a class able to produce or consume configuration
    /// </summary>
    /// <typeparam name="T">Configuration type.</typeparam>
    public interface IConfigurableEnvironment<TOutput> where TOutput : class
    {
        /// <summary>
        /// Gets or sets the configuration produced.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public TOutput OutputConfiguration { get; set; }
    }

    public interface IConfigurableEnvironment<TOutput, TInput> : IConfigurableEnvironment<TOutput> where TOutput : class where TInput : class
    {
        /// <summary>
        /// Gets or sets the configuration produced.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public TInput InputConfiguration { get; }
    }

    ///<inheritdoc/>
    public interface IConfigurableEnvironment : IConfigurableEnvironment<Dictionary<string, string>>
    {
    }
}

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

    ///<inheritdoc/>
    public interface IConfigurableEnvironment : IConfigurableEnvironment<Dictionary<string, string>>
    {
    }
}

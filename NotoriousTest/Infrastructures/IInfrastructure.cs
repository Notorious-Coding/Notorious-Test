namespace NotoriousTest.Infrastructures
{
    /// <summary>
    /// Defines the contract for a test infrastructure component managed by an environment.
    /// </summary>
    public interface IInfrastructure
    {
        /// <summary>Gets or sets whether the infrastructure is automatically reset between tests.</summary>
        bool AutoReset { get; set; }

        /// <summary>Gets or sets the context identifier linking this infrastructure to its environment.</summary>
        ContextId ContextId { get; set; }

        /// <summary>Gets the initialization order of this infrastructure; lower values initialize first.</summary>
        int? Order { get; }

        /// <summary>Destroys the infrastructure and releases all associated resources.</summary>
        Task Destroy();

        /// <summary>Initializes the infrastructure.</summary>
        Task Initialize();

        /// <summary>Resets the infrastructure to a clean state between tests.</summary>
        Task Reset();
    }
}

namespace NotoriousTest.Common.Infrastructures
{
    public interface IInfrastructure
    {
        /// <summary>
        /// Allows defining the order in which the infrastructure will be executed within the environment.
        /// </summary>
        int? Order { get; }

        /// <summary>
        /// AutoReset is a flag to reset the infrastructure after each test. Default is true.
        /// </summary>
        bool AutoReset { get; set; }

        /// <summary>
        /// ContextId is an identfier of the current infrastructure context.
        /// In standalone mode : ContextId is a unique identifier scoped for the infrastructure.
        /// Within an environment : ContextId is provided by the environment when the infrastructure is added.
        /// </summary>
        public Guid ContextId { get; set; }
    }
}

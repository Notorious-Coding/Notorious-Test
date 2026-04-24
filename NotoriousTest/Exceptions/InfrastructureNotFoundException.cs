namespace NotoriousTest.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested infrastructure is not found within an environment.
    /// </summary>
    public class InfrastructureNotFoundException : Exception
    {
        /// <summary>Initializes a new instance of <see cref="InfrastructureNotFoundException"/>.</summary>
        public InfrastructureNotFoundException()
        {
        }

        /// <summary>Initializes a new instance of <see cref="InfrastructureNotFoundException"/> with a message.</summary>
        /// <param name="message">The error message.</param>
        public InfrastructureNotFoundException(string? message) : base(message)
        {
        }

        /// <summary>Initializes a new instance of <see cref="InfrastructureNotFoundException"/> with a message and inner exception.</summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public InfrastructureNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}

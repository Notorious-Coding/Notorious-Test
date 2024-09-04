namespace NotoriousTest.Common.Exceptions
{
    public class InfrastructureNotFoundException : Exception
    {
        public InfrastructureNotFoundException()
        {
        }

        public InfrastructureNotFoundException(string? message) : base(message)
        {
        }

        public InfrastructureNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}

using System.Runtime.Serialization;

namespace NotoriousTest.Core.Environments
{
    [Serializable]
    internal class RuntimeNotFoundException : Exception
    {
        public RuntimeNotFoundException()
        {
        }

        public RuntimeNotFoundException(string message) : base(message)
        {
        }

        public RuntimeNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RuntimeNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
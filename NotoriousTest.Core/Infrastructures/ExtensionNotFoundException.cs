using System.Runtime.Serialization;

namespace NotoriousTest.Core.Infrastructures
{
    [Serializable]
    internal class ExtensionNotFoundException : Exception
    {
        public ExtensionNotFoundException()
        {
        }

        public ExtensionNotFoundException(string message) : base(message)
        {
        }

        public ExtensionNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ExtensionNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
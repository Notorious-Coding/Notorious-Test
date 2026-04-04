using System.Runtime.Serialization;

namespace NotoriousTest.Core.Extensions
{
    [Serializable]
    internal class SectionNotFoundException : Exception
    {
        public SectionNotFoundException()
        {
        }

        public SectionNotFoundException(string message) : base(message)
        {
        }

        public SectionNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SectionNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
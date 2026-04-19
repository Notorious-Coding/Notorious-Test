using System.Runtime.Serialization;

namespace NotoriousTest.Core.Infrastructures
{
    [Serializable]
    internal class InfrastructureSettingsNotFound : Exception
    {
        public InfrastructureSettingsNotFound()
        {
        }

        public InfrastructureSettingsNotFound(string message) : base(message)
        {
        }

        public InfrastructureSettingsNotFound(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InfrastructureSettingsNotFound(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

using NotoriousTest.Core;
using NotoriousTest.Core.Logger;

using Xunit.Sdk;
using Xunit.v3;

namespace NotoriousTest.XUnit.Logger
{
    public class TestLogger : ITestLogger
    {
        private readonly IMessageSink _sink;
        private readonly ContextId _contextId;

        public TestLogger(IMessageSink sink, ContextId contextId)
        {
            _sink = sink;
            _contextId = contextId;
        }

        public void Log(string message)
        {
            _sink.OnMessage(new DiagnosticMessage($"[NotoriousTest][{_contextId.Value.ToString()}]{message}"));
        }
    }
}

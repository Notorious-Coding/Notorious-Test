using Xunit.Sdk;
using Xunit.v3;

namespace NotoriousTest.Logger
{
    public class TestLogger : ITestLogger
    {
        private readonly IMessageSink _sink;

        public TestLogger(IMessageSink sink)
        {
            _sink = sink;
        }

        public void Log(string message)
        {
            _sink.OnMessage(new DiagnosticMessage(message));
        }
    }
}

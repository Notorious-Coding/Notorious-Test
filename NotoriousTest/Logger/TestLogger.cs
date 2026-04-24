using Xunit.Sdk;
using Xunit.v3;

namespace NotoriousTest.Logger
{
    /// <summary>
    /// xUnit implementation of <see cref="ITestLogger"/> that emits messages via <see cref="IMessageSink"/>.
    /// </summary>
    public class TestLogger : ITestLogger
    {
        private readonly IMessageSink _sink;
        private readonly ContextId _contextId;

        /// <summary>Initializes a new instance with the given message sink and context identifier.</summary>
        /// <param name="sink">The xUnit message sink used to emit diagnostic messages.</param>
        /// <param name="contextId">The context identifier included in each log message.</param>
        public TestLogger(IMessageSink sink, ContextId contextId)
        {
            _sink = sink;
            this._contextId = contextId;
        }

        /// <inheritdoc/>
        public void Log(string message)
        {
            _sink.OnMessage(new DiagnosticMessage($"[NotoriousTest][{_contextId.Value.ToString()}]{message}"));
        }
    }
}

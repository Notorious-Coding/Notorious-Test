using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace NotoriousTest.XUnit.Logger
{
    public class XUnitTestLogger : ITestLogger
    {
        private readonly EnvironmentId _contextId;

        public XUnitTestLogger(EnvironmentId contextId)
        {
            _contextId = contextId;
        }

        public void Log(string message) => TestContext.Current.SendDiagnosticMessage($"[NotoriousTest][{_contextId.Value.ToString()}]{message}");
    }
}

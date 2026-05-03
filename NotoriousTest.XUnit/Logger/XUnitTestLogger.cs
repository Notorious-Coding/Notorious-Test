using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace NotoriousTest.XUnit.Logger
{
    public class XUnitTestLogger : ITestLogger
    {
        public void Log(string message, EnvironmentId environmentId) => TestContext.Current.SendDiagnosticMessage($"[NotoriousTest][{environmentId.Value.ToString()}]{message}");
    }
}

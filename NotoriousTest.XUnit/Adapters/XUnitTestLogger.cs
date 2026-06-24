using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using Xunit;

namespace NotoriousTest.XUnit.Logger;

public class XUnitTestLogger : ITestLogger
{
    public void Log(string message, EnvironmentId environmentId) =>
        TestContext.Current.SendDiagnosticMessage($"[NotoriousTest][{environmentId.Value.ToString()}]{message}");
}

using NotoriousTest.Core;
using NotoriousTest.Core.Logger;

namespace NotoriousTest.TUnit.Logger
{
    public class TUnitTestLogger : ITestLogger
    {
        public void Log(string message, EnvironmentId environment)
        {
            TestContext.Current!.Output.WriteLine($"[NotoriousTest][{environment.Value}]{message}");
        }
    }
}

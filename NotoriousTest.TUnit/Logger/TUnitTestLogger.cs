using NotoriousTest.Core;
using NotoriousTest.Core.Logger;

namespace NotoriousTest.TUnit.Logger
{
    public class TUnitTestLogger : ITestLogger
    {
        private readonly EnvironmentId _contextId;

        public TUnitTestLogger(EnvironmentId contextId)
        {
            _contextId = contextId;
        }

        public void Log(string message)
        {
            TestContext.Current!.Output.WriteLine($"[NotoriousTest][{_contextId.Value}]{message}");
        }
    }
}

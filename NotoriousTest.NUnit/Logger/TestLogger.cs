using NUnit.Framework;

using NotoriousTest.Core;
using NotoriousTest.Core.Logger;

namespace NotoriousTest.NUnit.Logger
{
    public class TestLogger : ITestLogger
    {
        private readonly EnvironmentId _contextId;

        public TestLogger(EnvironmentId contextId)
        {
            _contextId = contextId;
        }

        public void Log(string message)
        {
            TestContext.Progress.WriteLine($"[NotoriousTest][{_contextId.Value}]{message}");
        }
    }
}

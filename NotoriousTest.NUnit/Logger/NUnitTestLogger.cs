using NUnit.Framework;

using NotoriousTest.Core;
using NotoriousTest.Core.Logger;

namespace NotoriousTest.NUnit.Logger
{
    public class NUnitTestLogger : ITestLogger
    {
        private readonly EnvironmentId _contextId;

        public NUnitTestLogger(EnvironmentId contextId)
        {
            _contextId = contextId;
        }

        public void Log(string message)
        {
            TestContext.Progress.WriteLine($"[NotoriousTest][{_contextId.Value}]{message}");
        }
    }
}

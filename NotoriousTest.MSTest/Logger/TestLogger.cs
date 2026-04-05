using Microsoft.VisualStudio.TestTools.UnitTesting;

using NotoriousTest.Core;
using NotoriousTest.Core.Logger;

namespace NotoriousTest.MSTest.Logger
{
    public class TestLogger : ITestLogger
    {
        private readonly TestContext _testContext;
        private readonly EnvironmentId _contextId;

        public TestLogger(TestContext testContext, EnvironmentId contextId)
        {
            _testContext = testContext;
            _contextId = contextId;
        }

        public void Log(string message)
        {
            _testContext.WriteLine($"[NotoriousTest][{_contextId.Value}]{message}");
        }
    }
}

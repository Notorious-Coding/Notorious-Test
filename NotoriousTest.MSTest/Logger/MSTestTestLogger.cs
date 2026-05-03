using Microsoft.VisualStudio.TestTools.UnitTesting;

using NotoriousTest.Core;
using NotoriousTest.Core.Logger;

namespace NotoriousTest.MSTest.Logger
{
    public class MSTestTestLogger : ITestLogger
    {
        private readonly TestContext _testContext;
        private readonly EnvironmentId _contextId;

        public MSTestTestLogger(TestContext testContext)
        {
            _testContext = testContext;
        }

        public void Log(string message, EnvironmentId environmentId)
        {
            _testContext.WriteLine($"[NotoriousTest][{environmentId.Value}]{message}");
        }
    }
}

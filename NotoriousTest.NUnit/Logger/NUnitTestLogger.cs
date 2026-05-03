using NUnit.Framework;

using NotoriousTest.Core;
using NotoriousTest.Core.Logger;

namespace NotoriousTest.NUnit.Logger
{
    public class NUnitTestLogger : ITestLogger
    {
        public void Log(string message, EnvironmentId environmentId)
        {
            TestContext.Progress.WriteLine($"[NotoriousTest][{environmentId.Value}]{message}");
        }
    }
}

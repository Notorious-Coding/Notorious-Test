namespace NotoriousTest.Core.Logger
{
    public interface ITestLogger
    {
        void Log(string message, EnvironmentId environmentId);
    }
}

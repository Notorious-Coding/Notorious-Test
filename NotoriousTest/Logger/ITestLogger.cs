namespace NotoriousTest.Logger
{
    /// <summary>
    /// Provides a mechanism for emitting diagnostic log messages during test infrastructure lifecycle.
    /// </summary>
    public interface ITestLogger
    {
        /// <summary>Logs a diagnostic message.</summary>
        /// <param name="message">The message to log.</param>
        void Log(string message);
    }
}

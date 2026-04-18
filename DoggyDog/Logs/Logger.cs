using System.Text;

namespace DoggyDog.Logs
{
    public class Logger
    {

        private static Logger _logger;

        public static Logger Instance
        {
            get
            {
                return _logger ??= new Logger();
            }
        }

        private readonly Stack<string> _scopeStack = new();
        public static LogLevel MinLogLevel { get; set; } = LogLevel.Info;
        public LogScope CreateScope(string scopeName, string? message = null)
        {
            if (!string.IsNullOrWhiteSpace(message)) Trace(message);
            _scopeStack.Push(scopeName);
            return new LogScope(this, scopeName);
        }

        public void ExitScope(string name)
        {
            _scopeStack.TryPop(out _);
        }
        public void Log(LogLevel logType, string message, Exception? exception = null)
        {
            if (logType > MinLogLevel) return;
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(" > ");
            var scopeString = string.Join(string.Empty, _scopeStack.Reverse().Select(scope => $"[{scope}]").ToArray());

            stringBuilder.Append(scopeString);

            Action<Action> colorScope = logType switch
            {
                LogLevel.Error => LogColorScope.Red,
                LogLevel.Warning => LogColorScope.Yellow,
                LogLevel.Info => LogColorScope.Cyan,
                LogLevel.Success => LogColorScope.Green,
                LogLevel.Trace => LogColorScope.Gray,
                LogLevel.Debug => LogColorScope.DarkGray,
                _ => LogColorScope.Gray
            };

            colorScope(() =>
            {
                stringBuilder.Append(" ");
                stringBuilder.Append(message);
                if (exception != null) stringBuilder.Append(exception.ToString());
                Console.WriteLine(stringBuilder.ToString());
            });
        }

        public void Error(string message, Exception? exception = null) => Log(LogLevel.Error, message, exception);
        public void Warning(string message, Exception? exception = null) => Log(LogLevel.Warning, message, exception);
        public void Info(string message, Exception? exception = null) => Log(LogLevel.Info, message, exception);
        public void Success(string message, Exception? exception = null) => Log(LogLevel.Success, message, exception);
        public void Trace(string message, Exception? exception = null) => Log(LogLevel.Trace, message, exception);
        public void Debug(string message, Exception? exception = null) => Log(LogLevel.Debug, message, exception);
    }
    public class LogScope(Logger logger, string name) : IDisposable
    {
        public void Dispose()
        {
            logger.ExitScope(name);
        }
    }
    public enum LogLevel
    {
        Error,
        Warning,
        Success,
        Info,
        Trace,
        Debug
    }
}

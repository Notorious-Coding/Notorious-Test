using System.Text;

namespace DoggyDog.Logs;

public class Logger
{
    private static readonly Lazy<Logger> _logger = new(() => new Logger());
    private static readonly Lock _consoleLock = new();

    // Each execution flow (thread / async flow) has its own scope chain, inherited from its parent flow.
    private readonly AsyncLocal<LogScope?> _currentScope = new();

    public static Logger Instance => _logger.Value;
    public static LogLevel MinLogLevel { get; set; } = LogLevel.Debug;

    public LogScope CreateScope(string scopeName, string? message = null)
    {
        if (!string.IsNullOrWhiteSpace(message)) Trace(message);
        var scope = new LogScope(this, scopeName, _currentScope.Value);
        _currentScope.Value = scope;
        return scope;
    }

    internal void RestoreScope(LogScope? scope) => _currentScope.Value = scope;

    public void Log(LogLevel logType, string message, Exception? exception = null)
    {
        if (logType > MinLogLevel) return;
        var stringBuilder = new StringBuilder();

        stringBuilder.Append(" > ");
        stringBuilder.Append(BuildScopeString());

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

        stringBuilder.Append(' ');
        stringBuilder.Append(message);
        if (exception != null) stringBuilder.Append(exception);
        string line = stringBuilder.ToString();

        lock (_consoleLock)
        {
            colorScope(() => Console.WriteLine(line));
        }
    }

    private string BuildScopeString()
    {
        var names = new Stack<string>();
        for (LogScope? scope = _currentScope.Value; scope != null; scope = scope.Parent)
            names.Push($"[{scope.Name}]");
        return string.Concat(names);
    }

    public void Error(string message, Exception? exception = null) => Log(LogLevel.Error, message, exception);

    public void Warning(string message, Exception? exception = null) => Log(LogLevel.Warning, message, exception);

    public void Info(string message, Exception? exception = null) => Log(LogLevel.Info, message, exception);

    public void Success(string message, Exception? exception = null) => Log(LogLevel.Success, message, exception);

    public void Trace(string message, Exception? exception = null) => Log(LogLevel.Trace, message, exception);

    public void Debug(string message, Exception? exception = null) => Log(LogLevel.Debug, message, exception);
}

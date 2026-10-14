namespace DoggyDog.Logs;

public sealed class LogScope(Logger logger, string name, LogScope? parent) : IDisposable
{
    public string Name { get; } = name;
    public LogScope? Parent { get; } = parent;

    public void Dispose() => logger.RestoreScope(Parent);
}

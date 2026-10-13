namespace DoggyDog.Logs;

public class LogScope(Logger logger, string name) : IDisposable
{
    public void Dispose() => logger.ExitScope(name);
}

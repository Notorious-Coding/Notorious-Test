using System.Reflection;
using DoggyDog.Watchdog.Logs;

namespace DoggyDog.Watchdog.Arguments;

public static class ArgumentsParser
{
    public static T Parse<T>(string[] args) where T : new()
    {
        var dict = args
            .Chunk(2)
            .Where(p => p.Length == 2 && p[0].StartsWith("--"))
            .ToDictionary(p => p[0].TrimStart('-'), p => p[1]);

        return ParseInternal<T>(prop => dict.TryGetValue(prop, out string? v) ? v : null);
    }

    public static T ParseFromEnv<T>(string envPrefix) where T : new() =>
        ParseInternal<T>(prop =>
        {
            string envKey = $"{envPrefix}_{prop.Replace("-", "_").ToUpperInvariant()}";
            return Environment.GetEnvironmentVariable(envKey, EnvironmentVariableTarget.User);
        });

    private static T ParseInternal<T>(Func<string, string?> resolver) where T : new()
    {
        int errors = 0;
        var instance = new T();
        foreach (PropertyInfo prop in typeof(T).GetProperties())
        {
            CliArgumentAttribute? attr = prop.GetCustomAttribute<CliArgumentAttribute>();
            if (attr is null) continue;

            string? raw = resolver(attr.Name);

            if (raw is null)
            {
                if (attr.Required)
                    Logger.Instance.Error($"--{attr.Name} is required.");
                continue;
            }

            try
            {
                Type targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                object converted = targetType switch
                {
                    _ when targetType == typeof(Guid) => Guid.Parse(raw),
                    _ when targetType == typeof(DateTimeOffset) => DateTimeOffset.Parse(raw),
                    _ when targetType.IsEnum => Enum.Parse(targetType, raw),
                    _ when targetType == typeof(string[]) => raw.Split("|"),
                    _ => Convert.ChangeType(raw, targetType)
                };


                prop.SetValue(instance, converted);
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Parameter --{attr.Name} is not valid.", ex);
                errors++;
            }
        }

        if (errors > 0)
        {
            Console.Read();
            Environment.Exit(1);
        }

        return instance;
    }
}

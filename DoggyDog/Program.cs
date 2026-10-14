using DoggyDog;
using DoggyDog.AssemblyLoader;
using DoggyDog.Logs;
using Microsoft.Data.Sqlite;
using NotoriousTest.Internal.SqlLiteRegistry;

Logger logger = Logger.Instance;
try
{
    using (logger.CreateScope("DoggyDog"))
    {
        if (args.Contains("--from-env"))
            logger.Debug("Parsing parameters from environment variables");

        WatchDogParameters parameters = args.Contains("--from-env")
            ? ArgumentsParser.ParseFromEnv<WatchDogParameters>("DOGGYDOG_DEBUG")
            : ArgumentsParser.Parse<WatchDogParameters>(args);

        if (parameters.LogLevel != null)
            Logger.MinLogLevel = parameters.LogLevel.Value;

        DoggyDogRecoveryWatchdog.Banner(parameters.Pid, parameters.EnvironmentId);
        SqliteRegistryProvider registry = GetRegistry(parameters);

        var assemblyLoader = new TestAssemblyLoader(parameters.AssemblyPath, parameters.RuntimesPath);
        var watchdog = new DoggyDogRecoveryWatchdog(assemblyLoader, registry, logger);

        await watchdog.Run(parameters.Pid, parameters.EnvironmentId);
    }
}
catch (Exception ex)
{
    logger.Error("Fatal error during execution.", ex);
    ConsoleExit.ExitAfterDelay(1, Logger.Instance);
}

static SqliteRegistryProvider GetRegistry(WatchDogParameters parameters)
{
    var cs = new SqliteConnectionStringBuilder(parameters.ConnectionString); // Validate connection string format early

    if (!File.Exists(cs.DataSource))
    {
        Logger.Instance.Error($"Registry file not found at {cs.DataSource}");
        ConsoleExit.ExitAfterDelay(1, Logger.Instance);
    }
    else
    {
        Logger.Instance.Info($"Using registry file at {cs.DataSource}");
    }

    var registry = new SqliteRegistryProvider(new SqliteRegistryProviderConfiguration
    {
        ConnectionString = parameters.ConnectionString
    });
    return registry;
}

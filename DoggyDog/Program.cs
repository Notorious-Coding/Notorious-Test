using DoggyDog;
using DoggyDog.AssemblyLoader;
using DoggyDog.Logs;

using Microsoft.Data.Sqlite;

using NotoriousTest.SqlLiteRegistry;

Logger logger = Logger.Instance;
try
{
    using (logger.CreateScope("DoggyDog"))
    {
        WatchDogParameters parameters;

        if (args.Contains("--from-env"))
            parameters = ArgumentsParser.ParseFromEnv<WatchDogParameters>("DOGGYDOG_DEBUG");
        else
            parameters = ArgumentsParser.Parse<WatchDogParameters>(args);

        if (parameters.LogLevel != null)
            Logger.MinLogLevel = parameters.LogLevel.Value;

        DoggyDogRecoveryWatchdog.Banner(parameters.Pid, parameters.EnvironmentId);
        SqliteRegistryProvider registry = GetRegistry(parameters);

        TestAssemblyLoader assemblyLoader = new TestAssemblyLoader(parameters.AssemblyPath, parameters.RuntimesPath);
        var watchdog = new DoggyDogRecoveryWatchdog(assemblyLoader, registry, logger);

        await watchdog.Run(parameters.Pid, parameters.EnvironmentId);

    }
}
catch (Exception ex)
{

    logger.Error($"Fatal error during execution.", ex);
    Console.WriteLine("Press any key to exit...");
    Console.Read();
    Environment.Exit(1);
}

static SqliteRegistryProvider GetRegistry(WatchDogParameters parameters)
{
    var cs = new SqliteConnectionStringBuilder(parameters.ConnectionString); // Validate connection string format early

    if (!File.Exists(cs.DataSource))
    {
        Logger.Instance.Error($"Registry file not found at {cs.DataSource}");
        Environment.Exit(1);
    }
    else
    {
        Logger.Instance.Info($"Using registry file at {cs.DataSource}");
    }

    SqliteRegistryProvider registry = new SqliteRegistryProvider(new SqliteRegistryProviderConfiguration()
    {
        ConnectionString = parameters.ConnectionString
    });
    return registry;
}
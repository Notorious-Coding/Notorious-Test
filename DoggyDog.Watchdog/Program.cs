using System;
using System.IO;
using DoggyDog;
using DoggyDog.Watchdog;
using DoggyDog.Watchdog.Arguments;
using DoggyDog.Watchdog.AssemblyLoader;
using DoggyDog.Watchdog.Logs;
using Microsoft.Data.Sqlite;
using NotoriousTest.SqlLiteRegistry;

Logger logger = Logger.Instance;
try
{
    using (logger.CreateScope("DoggyDog.Watchdog"))
    {
        if (args.Contains("--from-env"))
            logger.Debug("Parsing parameters from environment variables");

        WatchDogParameters parameters = args.Contains("--from-env")
            ? ArgumentsParser.ParseFromEnv<WatchDogParameters>("DOGGYDOG_DEBUG")
            : ArgumentsParser.Parse<WatchDogParameters>(args);

        if (parameters.LogLevel != null)
            Logger.MinLogLevel = parameters.LogLevel.Value;

        DoggyDogRecoveryWatchdog.Banner(parameters.Pid, parameters.EnvironmentId);
        SqliteRegistryRepository registry = GetRegistry(parameters);

        var assemblyLoader = new TestAssemblyLoader(parameters.AssemblyPath, parameters.RuntimesPath);
        var watchdog = new DoggyDogRecoveryWatchdog(assemblyLoader, registry, logger);

        await watchdog.Run(parameters.Pid, parameters.EnvironmentId);
    }
}
catch (Exception ex)
{
    logger.Error("Fatal error during execution.", ex);
    Console.WriteLine("Press any key to exit...");
    Console.Read();
    Environment.Exit(1);
}

static SqliteRegistryRepository GetRegistry(WatchDogParameters parameters)
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

    var registry = new SqliteRegistryRepository(new SqliteRegistryRepositoryConfiguration
    {
        ConnectionString = parameters.ConnectionString
    });
    return registry;
}

using System.Data.Common;
using Dapper;
using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Settings;
using NotoriousTest.Sqlite;

namespace NotoriousTest.IntegrationTests.SystemUnderTest;

public class NotoriousTestRegistryInfrastructure(
    EnvironmentId contextId,
    ITestSettingsProvider settingsProvider,
    ITestLogger logger,
    IRegistry registry)
    : SqliteInfrastructure(contextId, settingsProvider, logger, registry)
{
    public override async Task Initialize()
    {
        await base.Initialize();
        await using DbConnection connection = GetDatabaseConnection();
        await connection.ExecuteAsync(ENSURE_REGISTRY);
    }

    private const string ENSURE_REGISTRY = @$"
                CREATE TABLE IF NOT EXISTS InfrastructureRegistry(
                    InfrastructureId TEXT PRIMARY KEY,
                    InfrastructureType TEXT NOT NULL,
                    EnvironmentId TEXT NOT NULL,
                    ProcessID TEXT NOT NULL,
                    Metadata TEXT,
                    MetadataType TEXT,
                    CreationDate TEXT NOT NULL,
                    UpdateDate TEXT NOT NULL,
                    LastResetDate TEXT
                )
            ";
}

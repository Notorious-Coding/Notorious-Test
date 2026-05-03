using NotoriousTest.Core;
using NotoriousTest.Core.Infrastructures;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;

using System.Data.Common;
using Respawn;
using Respawn.Graph;

namespace NotoriousTest.Database
{

    public abstract class DatabaseInfrastructureBase<TOutputConfiguration, TMetadata> : Infrastructure<TOutputConfiguration, TMetadata>, IDatabaseInfrastructure where TMetadata : class
    {
        public string DbPrefix { get; init; } = "NotoriousDb";
        public string FullDbName => $"{DbPrefix}_{EnvironmentId.Value}";
        public string[] TableToIgnore { get; init; } = [];
        public string[] TableToInclude { get; init; } = [];
        public string[] SchemasToInclude { get; init; } = [];
        public string[] SchemasToExclude { get; init; } = [];

        private Respawner? _respawner;


        public DatabaseInfrastructureBase(EnvironmentId contextId, ITestLogger logger, IRegistry registry) : base(contextId, logger, registry)
        {
        }


        public abstract DbConnection GetConnection(string connectionString);
        /// <summary>
        /// Returns a SQL Server connection connected to the current infrastructure's database.
        /// </summary>
        /// <returns>A SqlConnection instance connected to the current infrastructure's database.</returns>
        public DbConnection GetDatabaseConnection() => GetConnection(GetDatabaseConnectionString());

        public DbConnection GetServerConnection() => GetConnection(GetServerConnectionString());

        /// <summary>
        /// Returns a SQL Server connection string pointing to the current infrastructure's database.
        /// </summary>
        /// <returns>A SqlConnection instance pointing to the current infrastructure's database.</returns>
        public abstract string GetDatabaseConnectionString();
        public abstract string GetServerConnectionString();

        public override async Task Initialize()
        {
            await using DbConnection connection = GetServerConnection();
            await connection.OpenAsync();
            await CreateDatabase(connection);

            await using DbConnection databaseConnection = GetDatabaseConnection();
            await databaseConnection.OpenAsync();
        }

        public override async Task Reset()
        {

            try
            {
                await using DbConnection connection = GetDatabaseConnection();
                await connection.OpenAsync();

                _respawner ??= await Respawner.CreateAsync(connection, new RespawnerOptions()
                {
                    TablesToIgnore = TableToIgnore.Select(tti => new Table(tti)).ToArray(),
                    TablesToInclude = TableToInclude.Select(tti => new Table(tti)).ToArray(),
                    SchemasToExclude = SchemasToExclude,
                    SchemasToInclude = SchemasToInclude
                });

                await _respawner.ResetAsync(connection);
            }
            catch (InvalidOperationException ex)
            {
                // This can occur if the database has no tables. In that case, we can ignore the exception and continue with the test setup.
                Logger.Log(ex.Message, EnvironmentId);
            }


        }

        public override async Task Destroy()
        {
            await using DbConnection connection = GetServerConnection();

            await connection.OpenAsync();
            await DropDatabase(connection);
        }

        protected abstract Task CreateDatabase(DbConnection sqlConnection);
        protected abstract Task DropDatabase(DbConnection sqlConnection);
    }
}

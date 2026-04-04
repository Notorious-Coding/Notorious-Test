using NotoriousTest.Core;
using NotoriousTest.Core.Infrastructures;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;

using System.Data.Common;

namespace NotoriousTest.Database
{

    public abstract class DatabaseInfrastructureBase<TOutputConfiguration, TMetadata> : Infrastructure<TOutputConfiguration, TMetadata>, IDatabaseInfrastructure where TMetadata : class
    {
        public string DbPrefix { get; init; } = "NotoriousDb";
        public string FullDbName => $"{DbPrefix}_{ContextId.Value}";
        public string[] TableToIgnore { get; init; } = [];
        public string[] TableToInclude { get; init; } = [];


        public DatabaseInfrastructureBase(ContextId contextId, ITestLogger logger, IRegistry registry) : base(contextId, logger, registry)
        {
        }


        public abstract DbConnection GetConnection(string connectionString);
        /// <summary>
        /// Returns a SQL Server connection connected to the current infrastructure's database.
        /// </summary>
        /// <returns>A SqlConnection instance connected to the current infrastructure's database.</returns>
        public DbConnection GetDatabaseConnection()
        {
            return GetConnection(GetDatabaseConnectionString());
        }

        public DbConnection GetServerConnection()
        {
            return GetConnection(GetServerConnectionString());
        }

        /// <summary>
        /// Returns a SQL Server connection string pointing to the current infrastructure's database.
        /// </summary>
        /// <returns>A SqlConnection instance pointing to the current infrastructure's database.</returns>
        public abstract string GetDatabaseConnectionString();
        public abstract string GetServerConnectionString();

        public override async Task Initialize()
        {
            using var connection = GetServerConnection();
            await connection.OpenAsync();
            await CreateDatabase(connection);
        }

        public override async Task Reset()
        {
        }

        public override async Task Destroy()
        {
            using var connection = GetServerConnection();

            await connection.OpenAsync();
            await DropDatabase(connection);
        }

        protected abstract Task CreateDatabase(DbConnection sqlConnection);
        protected abstract Task DropDatabase(DbConnection sqlConnection);
    }
}

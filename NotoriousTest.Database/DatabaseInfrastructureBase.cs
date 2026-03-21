using NotoriousTest.Infrastructures;

using System.Data.Common;

namespace NotoriousTest.Database
{

    public abstract class DatabaseInfrastructureBase<TOutputConfiguration> : Infrastructure<TOutputConfiguration>, IDatabaseInfrastructure
    {
        public string DbPrefix { get; init; } = "NotoriousDb";
        public string FullDbName => $"{DbPrefix}_{ContextId}";
        public string[] TableToIgnore { get; init; } = [];
        public string[] TableToInclude { get; init; } = [];


        public DatabaseInfrastructureBase() : base()
        {
        }

        /// <summary>
        /// Returns a SQL Server connection connected to the current infrastructure's database.
        /// </summary>
        /// <returns>A SqlConnection instance connected to the current infrastructure's database.</returns>
        public abstract DbConnection GetDatabaseConnection();
        public abstract DbConnection GetServerConnection();

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

using Microsoft.Data.SqlClient;

using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Database;

using Respawn;
using Respawn.Graph;

using System.Data.Common;

using Testcontainers.MsSql;

namespace NotoriousTest.SqlServer
{

    public class SqlServerContainerInfrastructure : SqlServerContainerInfrastructure<string>
    {
        public SqlServerContainerInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry) : base(contextId, logger, registry)
        {
        }

        /// <summary>
        /// Gets the configuration key used to retrieve the default connection string.
        /// </summary>
        /// <remarks>Override this property in a derived class to specify a different configuration key if
        /// needed.</remarks>
        protected virtual string ConnectionStringKey => "ConnectionStrings:Default";
        public override async Task Initialize()
        {
            await base.Initialize();

            AddEntry(ConnectionStringKey, GetDatabaseConnectionString());
        }
    }

    public class SqlServerContainerInfrastructure<TOutputConfiguration> : DockerDatabaseInfrastructure<MsSqlContainer, TOutputConfiguration>
    {
        public string[] SchemasToInclude { get; init; } = [];
        public string[] SchemasToExclude { get; init; } = [];

        public SqlServerContainerInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry) : base(contextId, logger, registry)
        {
            Container = ConfigureSqlContainer(new MsSqlBuilder()).Build();
            EnsureExtension(new RespawnExtension(() => new RespawnerOptions()
            {
                TablesToIgnore = TableToIgnore.Select(tti => new Table(tti)).ToArray(),
                TablesToInclude = TableToInclude.Select(tti => new Table(tti)).ToArray(),
                SchemasToExclude = SchemasToExclude,
                SchemasToInclude = SchemasToInclude,
                DbAdapter = DbAdapter.SqlServer
            }));
        }

        protected virtual MsSqlBuilder ConfigureSqlContainer(MsSqlBuilder builder)
        {
            return builder;
        }

        public override DbConnection GetConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public override string GetDatabaseConnectionString()
        {
            SqlConnectionStringBuilder connectionString = new SqlConnectionStringBuilder(Container.GetConnectionString());
            if (!string.IsNullOrEmpty(FullDbName))
            {
                connectionString.InitialCatalog = FullDbName;
            }

            return connectionString.ToString();
        }

        protected override async Task CreateDatabase(DbConnection sqlConnection)
        {

            using (DbCommand command = sqlConnection.CreateCommand())
            {
                command.CommandText = $"CREATE DATABASE [{FullDbName}]";
                await command.ExecuteNonQueryAsync();
            }
        }

    }
}

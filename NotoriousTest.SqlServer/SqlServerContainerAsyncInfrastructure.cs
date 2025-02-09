﻿using Microsoft.Data.SqlClient;
using NotoriousTest.TestContainers;
using Respawn;
using System.Xml.Linq;
using Testcontainers.MsSql;

namespace NotoriousTest.SqlServer
{
    public class SqlServerContainerAsyncInfrastructure<TConfig> : ConfiguredDockerContainerAsyncInfrastructure<MsSqlContainer, TConfig> where TConfig: new()
    {
        public string DbName { get; init; } = "NotoriousDb";
        public RespawnerOptions? RespawnOptions { get; set; } = null;
        private string _fullDbName;
        private Respawner _respawner;
        public SqlServerContainerAsyncInfrastructure(bool initialize = false) : base(initialize)
        {
            Container = ConfigureSqlContainer(new MsSqlBuilder()).Build();
        }

        /// <summary>
        /// Returns a SQL Server connection connected to the current infrastructure's database.
        /// </summary>
        /// <returns>A SqlConnection instance connected to the current infrastructure's database.</returns>
        public SqlConnection GetDatabaseConnection() => new SqlConnection(GetConnectionString(_fullDbName));

        /// <summary>
        /// Returns a SQL Server connection string pointing to the current infrastructure's database.
        /// </summary>
        /// <returns>A SqlConnection instance pointing to the current infrastructure's database.</returns>
        public string GetDatabaseConnectionString() => GetConnectionString(_fullDbName);

        public override async Task Initialize()
        {
            await base.Initialize();

            _fullDbName = $"{DbName}_{ContextId}";
            using (var connection = GetSqlConnection())
            {
                await connection.OpenAsync();
                await CreateDatabase(connection);
                _respawner = await Respawner.CreateAsync(connection, RespawnOptions);
            }
        }

        public override async Task Reset()
        {
            using (var connection = GetSqlConnection())
            {
                await connection.OpenAsync();
                await _respawner.ResetAsync(connection);
            }
        }

        protected virtual MsSqlBuilder ConfigureSqlContainer(MsSqlBuilder builder)
        {
            return builder;
        }

        private async Task CreateDatabase(SqlConnection sqlConnection)
        {
            using (SqlCommand command = sqlConnection.CreateCommand())
            {
                command.CommandText = $"CREATE DATABASE [{_fullDbName}]";
                await command.ExecuteNonQueryAsync();
            }
        }
        private SqlConnection GetSqlConnection() => new SqlConnection(GetConnectionString());
        private string GetConnectionString(string? dbName = null)
        {
            SqlConnectionStringBuilder connectionString = new SqlConnectionStringBuilder(Container.GetConnectionString());
            if (!string.IsNullOrEmpty(dbName))
            {
                connectionString.InitialCatalog = _fullDbName;
            }

            return connectionString.ToString();
        }
    }

    public class SqlServerContainerAsyncInfrastructure : ConfiguredDockerContainerAsyncInfrastructure<MsSqlContainer>
    {
        public string DbName { get; init; } = "NotoriousDb";
        public RespawnerOptions? RespawnOptions { get; set; } = null;
        private string _fullDbName;
        private Respawner _respawner;
        public SqlServerContainerAsyncInfrastructure(bool initialize = false) : base(initialize)
        {
            Container = ConfigureSqlContainer(new MsSqlBuilder()).Build();
        }

        /// <summary>
        /// Returns a SQL Server connection connected to the current infrastructure's database.
        /// </summary>
        /// <returns>A SqlConnection instance connected to the current infrastructure's database.</returns>
        public SqlConnection GetDatabaseConnection() => new SqlConnection(GetConnectionString(_fullDbName));

        /// <summary>
        /// Returns a SQL Server connection string pointing to the current infrastructure's database.
        /// </summary>
        /// <returns>A SqlConnection instance pointing to the current infrastructure's database.</returns>
        public string GetDatabaseConnectionString() => GetConnectionString(_fullDbName);

        public override async Task Initialize()
        {
            await base.Initialize();

            _fullDbName = $"{DbName}_{ContextId}";
            using (var connection = GetSqlConnection())
            {
                await connection.OpenAsync();
                await CreateDatabase(connection);
                _respawner = await Respawner.CreateAsync(connection, RespawnOptions);
            }
        }

        public override async Task Reset()
        {
            using (var connection = GetSqlConnection())
            {
                await connection.OpenAsync();
                await _respawner.ResetAsync(connection);
            }
        }

        protected virtual MsSqlBuilder ConfigureSqlContainer(MsSqlBuilder builder)
        {
            return builder;
        }

        private async Task CreateDatabase(SqlConnection sqlConnection)
        {
            using (SqlCommand command = sqlConnection.CreateCommand())
            {
                command.CommandText = $"CREATE DATABASE [{_fullDbName}]";
                await command.ExecuteNonQueryAsync();
            }
        }
        private SqlConnection GetSqlConnection() => new SqlConnection(GetConnectionString());
        private string GetConnectionString(string? dbName = null)
        {
            SqlConnectionStringBuilder connectionString = new SqlConnectionStringBuilder(Container.GetConnectionString());
            if (!string.IsNullOrEmpty(dbName))
            {
                connectionString.InitialCatalog = _fullDbName;
            }

            return connectionString.ToString();
        }
    }
}

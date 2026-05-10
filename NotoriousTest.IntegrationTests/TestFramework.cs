using AwesomeAssertions;

using Dapper;

using Docker.DotNet;

using NotoriousTest.Core;
using NotoriousTest.Core.Infrastructures;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Database;
using NotoriousTest.Sqlite;

using System.Data.Common;
using NotoriousTest.Core.Environments;

namespace NotoriousTest.IntegrationTests
{
    public static partial class TestFramework
    {
        public static class Arrange
        {
            public class FakeInfrastructure : NotoriousTest.Core.Infrastructures.Infrastructure
            {
                public FakeInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
                    : base(contextId, logger, registry, new EnvironmentSettings()) { }

                public override Task Initialize() => Task.CompletedTask;
                public override Task Destroy() => Task.CompletedTask;
            }

            public static async Task CreateTableWithData(IDatabaseInfrastructure databaseInfrastructure)
            {
                await databaseInfrastructure.GetDatabaseConnection()
                    .ExecuteAsync(@"
                    CREATE TABLE TestTable (
                        Id INT PRIMARY KEY,
                        Name VARCHAR(100) NOT NULL
                    );

                    INSERT INTO TestTable (Id, Name) VALUES (1, 'Test Name 1');
                ");
            }
        }


        public static class Assert
        {
            public static async Task ContainerDoesNotExist(string id)
            {
                var client = new DockerClientConfiguration().CreateClient();
                var act = () => client.Containers.InspectContainerAsync(id);

                await act.Should().ThrowAsync<DockerContainerNotFoundException>();
            }

            public static async Task ContainerExist(string id)
            {
                var client = new DockerClientConfiguration().CreateClient();
                var act = () => client.Containers.InspectContainerAsync(id);

                await act.Should().NotThrowAsync();
            }

            public static async Task DatabaseExist(IDatabaseInfrastructure infrastructure)
            {
                var connection = infrastructure.GetDatabaseConnection();
                var act = () => connection.OpenAsync(TestContext.Current.CancellationToken);
                await act.Should().NotThrowAsync();
            }

            public static async Task TableShouldBeEmpty(IDatabaseInfrastructure databaseInfrastructure)
            {
                var count = await databaseInfrastructure.GetDatabaseConnection()
                    .ExecuteScalarAsync<int>("SELECT COUNT(*) FROM TestTable;");
                count.Should().Be(0);
            }

            public static async Task DatabaseNoLongerExist(IDatabaseInfrastructure databaseInfrastructure)
            {
                DbConnection dbConnection = databaseInfrastructure.GetDatabaseConnection();
                var act = () =>
                {
                    return dbConnection.OpenAsync();
                };
                await act.Should().ThrowAsync<DbException>();
                await dbConnection.CloseAsync();
            }

            public static async Task InfrastructureShouldBeRegistered(SqliteInfrastructure registryDatabase, Guid infrastructureId)
            {
                using var connection = registryDatabase.GetDatabaseConnection();
                var count = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM InfrastructureRegistry WHERE InfrastructureId = @InfrastructureId",
                    new { InfrastructureId = infrastructureId.ToString() });
                count.Should().Be(1);
            }

            public static async Task InfrastructureShouldHaveBeenReset(SqliteInfrastructure registryDatabase, Guid infrastructureId)
            {
                using var connection = registryDatabase.GetDatabaseConnection();
                var lastResetDate = await connection.ExecuteScalarAsync<string?>(
                    "SELECT LastResetDate FROM InfrastructureRegistry WHERE InfrastructureId = @InfrastructureId",
                    new { InfrastructureId = infrastructureId.ToString() });
                lastResetDate.Should().NotBeNull();
            }

            public static async Task InfrastructureShouldBeRemovedFromRegistry(SqliteInfrastructure registryDatabase, Guid infrastructureId)
            {
                using var connection = registryDatabase.GetDatabaseConnection();
                var count = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM InfrastructureRegistry WHERE InfrastructureId = @InfrastructureId",
                    new { InfrastructureId = infrastructureId.ToString() });
                count.Should().Be(0);
            }
        }
    }
}

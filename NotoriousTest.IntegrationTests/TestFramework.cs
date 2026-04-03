using AwesomeAssertions;

using Dapper;

using Docker.DotNet;

using NotoriousTest.Database;

using System.Data.Common;

namespace NotoriousTest.IntegrationTests
{
    public static partial class TestFramework
    {
        public static class Arrange
        {

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
        }
    }
}

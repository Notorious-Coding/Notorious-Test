using AwesomeAssertions;

using FakeItEasy;

using Microsoft.Extensions.Configuration;

using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Settings;
using NotoriousTest.PostgreSql;
namespace NotoriousTest.IntegrationTests.Sqlite
{
    public class SqliteInfrastructureTests
    {
        private ITestSettingsProvider _testSettingsProvider = A.Fake<ITestSettingsProvider>();
        public SqliteInfrastructureTests()
        {
            A.CallTo(() => _testSettingsProvider.Find()).Returns(new ConfigurationBuilder().AddInMemoryCollection([new($"{nameof(SqliteInfrastructure)}:ConnectionString", "DataSource=testdb.db")]).Build());
        }


        [Fact]
        public async Task Initialize_Should_Create_Database()
        {
            ContextId contextId = Guid.NewGuid();
            string dbPrefix = nameof(Initialize_Should_Create_Database);

            await using var infrastructure = new SqliteInfrastructure(contextId, _testSettingsProvider, A.Fake<ITestLogger>(), A.Fake<IRegistry>())
            {
                DbPrefix = dbPrefix,
            };

            await infrastructure.InitializeAsync();
            var cs = infrastructure.GetDatabaseConnectionString();

            string expectedDbName = $"{dbPrefix}_{contextId.Value.ToString()}";
            infrastructure.FullDbName.Should().Be(expectedDbName);
            cs.Should().Contain($"Data Source=testdb_{expectedDbName}.db");

            using var connection = infrastructure.GetDatabaseConnection();
            var act = () => connection.OpenAsync(TestContext.Current.CancellationToken);
            await act.Should().NotThrowAsync();
            File.Exists(connection.DataSource).Should().BeTrue();
            await connection.CloseAsync();

        }

        [Fact]
        public async Task Reset_Should_Empty_Database()
        {
            ContextId contextId = Guid.NewGuid();
            string dbPrefix = nameof(Reset_Should_Empty_Database);
            await using var infrastructure = new SqliteInfrastructure(contextId, _testSettingsProvider, A.Fake<ITestLogger>(), A.Fake<IRegistry>())
            {
                DbPrefix = dbPrefix,
            };

            await infrastructure.InitializeAsync();
            await TestFramework.Arrange.CreateTableWithData(infrastructure);

            await infrastructure.ResetAsync();
            await TestFramework.Assert.TableShouldBeEmpty(infrastructure);
        }


        [Fact]
        public async Task Destroy_Should_Delete_Database()
        {
            ContextId contextId = Guid.NewGuid();
            string dbPrefix = nameof(Destroy_Should_Delete_Database);
            var infrastructure = new SqliteInfrastructure(contextId, _testSettingsProvider, A.Fake<ITestLogger>(), A.Fake<IRegistry>())
            {
                DbPrefix = dbPrefix,
            };

            await infrastructure.InitializeAsync();
            var connection = infrastructure.GetDatabaseConnection();

            var act = () => connection.OpenAsync(TestContext.Current.CancellationToken);
            await act.Should().NotThrowAsync();
            await connection.CloseAsync();
            File.Exists(connection.DataSource).Should().BeTrue();

            await infrastructure.DestroyAsync();

            File.Exists(connection.DataSource).Should().BeFalse();
        }
    }
}

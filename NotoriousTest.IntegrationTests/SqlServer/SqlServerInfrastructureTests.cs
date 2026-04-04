using AwesomeAssertions;

using FakeItEasy;

using Microsoft.Extensions.Configuration;

using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Settings;
using NotoriousTest.IntegrationTests.Environment;
using NotoriousTest.XUnit;
namespace NotoriousTest.IntegrationTests.SqlServer
{

    public class SqlServerInfrastructureTests : IntegrationTest<SqlServerInfrastructureEnvironment>
    {
        private ITestSettingsProvider _testSettingsProvider = A.Fake<ITestSettingsProvider>();
        public SqlServerInfrastructureTests(SqlServerInfrastructureEnvironment environment) : base(environment)
        {
            string serverConnectionString = CurrentEnvironment.GetInfrastructure<Environment.SqlServerInfrastructure>().OutputConfiguration[0].Value;
            A.CallTo(() => _testSettingsProvider.Find()).Returns(new ConfigurationBuilder().AddInMemoryCollection([new($"{nameof(NotoriousTest.SqlServer.SqlServerInfrastructure)}:ConnectionString", serverConnectionString + ";Pooling=false")]).Build());
        }


        [Fact]
        public async Task Initialize_Should_Create_Database()
        {
            ContextId contextId = Guid.NewGuid();
            string dbPrefix = nameof(Initialize_Should_Create_Database);

            await using var infrastructure = new NotoriousTest.SqlServer.SqlServerInfrastructure(contextId, _testSettingsProvider, A.Fake<ITestLogger>(), A.Fake<IRegistry>())
            {
                DbPrefix = dbPrefix,
            };

            await infrastructure.InitializeAsync();
            var cs = infrastructure.GetDatabaseConnectionString();

            string expectedDbName = $"{dbPrefix}_{contextId.Value.ToString()}";
            infrastructure.FullDbName.Should().Be(expectedDbName);
            cs.Should().Contain($"Initial Catalog={expectedDbName}");

            using var connection = infrastructure.GetDatabaseConnection();
            var act = () => connection.OpenAsync(TestContext.Current.CancellationToken);
            await act.Should().NotThrowAsync();

            await connection.CloseAsync();
        }

        [Fact]
        public async Task Reset_Should_Empty_Database()
        {
            ContextId contextId = Guid.NewGuid();
            string dbPrefix = nameof(Reset_Should_Empty_Database);
            await using var infrastructure = new NotoriousTest.SqlServer.SqlServerInfrastructure(contextId, _testSettingsProvider, A.Fake<ITestLogger>(), A.Fake<IRegistry>())
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
            var infrastructure = new NotoriousTest.SqlServer.SqlServerInfrastructure(contextId, _testSettingsProvider, A.Fake<ITestLogger>(), A.Fake<IRegistry>())
            {
                DbPrefix = dbPrefix,
            };

            await infrastructure.InitializeAsync();
            var connection = infrastructure.GetDatabaseConnection();
            var act = () => connection.OpenAsync(TestContext.Current.CancellationToken);
            await act.Should().NotThrowAsync();
            await connection.CloseAsync();

            await infrastructure.DestroyAsync();
            await TestFramework.Assert.DatabaseNoLongerExist(infrastructure);

        }
    }
}

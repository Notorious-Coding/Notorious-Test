using AwesomeAssertions;

using FakeItEasy;

using NotoriousTest.Core;
using NotoriousTest.Core.Environments;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.SqlServer;
namespace NotoriousTest.IntegrationTests.SqlServer
{

    public class SqlServerContainerInfrastructuresTests
    {

        public SqlServerContainerInfrastructuresTests()
        {

        }


        [Fact]
        public async Task Initialize_Should_CreateContainerAndADatabase()
        {
            EnvironmentId contextId = Guid.NewGuid();
            string dbPrefix = nameof(Initialize_Should_CreateContainerAndADatabase);
            await using var infrastructure = new SqlServerContainerInfrastructure(contextId, A.Fake<ITestLogger>(), A.Fake<IRegistry>(), new EnvironmentSettings())
            {
                DbPrefix = dbPrefix,
            };

            await infrastructure.InitializeAsync();
            var cs = infrastructure.GetDatabaseConnectionString();

            string expectedDbName = $"{dbPrefix}_{contextId.Value.ToString()}";
            infrastructure.FullDbName.Should().Be(expectedDbName);
            cs.Should().Contain($"Initial Catalog={expectedDbName}");

            await TestFramework.Assert.DatabaseExist(infrastructure);
        }

        [Fact]
        public async Task Reset_Should_Empty_Database()
        {
            EnvironmentId contextId = Guid.NewGuid();
            string dbPrefix = nameof(Reset_Should_Empty_Database);
            await using var infrastructure = new SqlServerContainerInfrastructure(contextId, A.Fake<ITestLogger>(), A.Fake<IRegistry>(), new EnvironmentSettings())
            {
                DbPrefix = dbPrefix,
            };

            await infrastructure.InitializeAsync();
            await TestFramework.Arrange.CreateTableWithData(infrastructure);

            await infrastructure.ResetAsync();
            await TestFramework.Assert.TableShouldBeEmpty(infrastructure);
        }


        [Fact]
        public async Task Destroy_Should_Delete_Container()
        {
            EnvironmentId contextId = Guid.NewGuid();
            string dbPrefix = nameof(Destroy_Should_Delete_Container);
            var infrastructure = new SqlServerContainerInfrastructure(contextId, A.Fake<ITestLogger>(), A.Fake<IRegistry>(), new EnvironmentSettings())
            {
                DbPrefix = dbPrefix,
            };

            await infrastructure.InitializeAsync();
            var connection = infrastructure.GetDatabaseConnection();
            var act = () => connection.OpenAsync(TestContext.Current.CancellationToken);
            await act.Should().NotThrowAsync();

            await infrastructure.DestroyAsync();
            await TestFramework.Assert.DatabaseNoLongerExist(infrastructure);

        }
    }
}

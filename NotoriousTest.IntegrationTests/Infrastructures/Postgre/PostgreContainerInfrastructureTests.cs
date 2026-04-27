using AwesomeAssertions;

using FakeItEasy;

using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.PostgreSql;

namespace NotoriousTest.IntegrationTests.Postgre
{
    public class PostgreContainerInfrastructureTests
    {
        [Fact]
        public async Task Initialize_Should_CreateContainerAndADatabase()
        {
            EnvironmentId contextId = Guid.NewGuid();
            string dbPrefix = nameof(Initialize_Should_CreateContainerAndADatabase);
            await using var infrastructure = new PostgreContainerInfrastructure(contextId, A.Fake<ITestLogger>(), A.Fake<IRegistry>())
            {
                DbPrefix = dbPrefix,
            };

            await infrastructure.InitializeAsync();
            var cs = infrastructure.GetDatabaseConnectionString();

            string expectedDbName = $"{dbPrefix}_{contextId.Value.ToString()}";
            infrastructure.FullDbName.Should().Be(expectedDbName);
            cs.Should().Contain($"Database={expectedDbName}");

            var connection = infrastructure.GetDatabaseConnection();
            var act = () => connection.OpenAsync(TestContext.Current.CancellationToken);
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Reset_Should_Empty_Database()
        {
            EnvironmentId contextId = Guid.NewGuid();
            string dbPrefix = nameof(Reset_Should_Empty_Database);
            await using var infrastructure = new PostgreContainerInfrastructure(contextId, A.Fake<ITestLogger>(), A.Fake<IRegistry>())
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
            var infrastructure = new PostgreContainerInfrastructure(contextId, A.Fake<ITestLogger>(), A.Fake<IRegistry>())
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

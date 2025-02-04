using Microsoft.Data.SqlClient;
using NotoriousTest.Common;
using NotoriousTests.InfrastructuresSamples.Environments;
using NotoriousTests.InfrastructuresSamples.Infrastructures;

namespace NotoriousTests.InfrastructuresSamples
{
    public class SampleTests : AsyncIntegrationTest<TestEnvironment>
    {
        public SampleTests(TestEnvironment environment) : base(environment)
        {
        }

        [Fact]
        public async Task Test1()
        {
            SqlServerInfrastructures sqlInfrastructure = await CurrentEnvironment.GetInfrastructureAsync<SqlServerInfrastructures>();
            await using(SqlConnection sql = sqlInfrastructure.GetConnection())
            {

                // Arrange database

                // Then act with a call to the API
                HttpClient client = (await CurrentEnvironment.GetWebApplication()).HttpClient;
                HttpResponseMessage response = await client.GetAsync("WeatherForecast");

                // Then assert that the database is in the expected state
                Assert.True(response.IsSuccessStatusCode);
            }

        }
    }
}
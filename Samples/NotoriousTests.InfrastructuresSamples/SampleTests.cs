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
            // Do not hesitate to create your test framework for your app, that will use the environment.
            // Example : new MyAppTestFramework(environment);
            // Then, you could create multiple methods to assert, arrange, act
            // that you could do multiple times in your tests.
        }

        [Fact]
        public async Task Test1()
        {
            // You can access an infrastructure directly from the CurrentEnvironment property of the test class.
            // This is useful to access the database connection for example.
            HttpClient client = (await CurrentEnvironment.GetWebApplication()).HttpClient;
            HttpResponseMessage response = await client.PostAsync("users", null);

            // Then assert that the database is in the expected state
            Assert.True(response.IsSuccessStatusCode);

            SqlServerInfrastructure sqlInfrastructure = await CurrentEnvironment.GetInfrastructureAsync<SqlServerInfrastructure>();
            await using(SqlConnection sql = sqlInfrastructure.GetDatabaseConnection())
            {
                // Then act with a call to the API
                await sql.OpenAsync();
                using (SqlCommand command = sql.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM Users";
                    int count = (int)await command.ExecuteScalarAsync();
                    Assert.Equal(1, count);
                }
            }
        }
    }
}
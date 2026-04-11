using NotoriousTest.Web;
using NotoriousTest.XUnit;

using System.Data.Common;

using Xunit;

namespace NotoriousTest.Sample.With
{
    public class WithNotoriousTestSample : IntegrationTest<MyEnvironment>
    {
        public WithNotoriousTestSample(MyEnvironment environment) : base(environment)
        {
        }

        [Fact]
        public async Task Test1()
        {
            // You can access an infrastructure directly from the CurrentEnvironment property of the test class.
            // This is useful to access the database connection for example.
            HttpClient client = CurrentEnvironment.GetWebApplication().HttpClient;
            HttpResponseMessage response = await client.PostAsync("users", null);

            // Then assert that the database is in the expected state
            Assert.True(response.IsSuccessStatusCode);

            MySqlServerInfrastructure sqlInfrastructure = CurrentEnvironment.GetInfrastructure<MySqlServerInfrastructure>();
            await using (DbConnection sql = sqlInfrastructure.GetDatabaseConnection())
            {
                // Then act with a call to the API
                await sql.OpenAsync(TestContext.Current.CancellationToken);
                using (DbCommand command = sql.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM Users";
                    int count = (int)await command.ExecuteScalarAsync(TestContext.Current.CancellationToken);
                    Assert.Equal(1, count);
                }
            }
        }

        [Fact]
        public async Task Test2()
        {
            // You can access an infrastructure directly from the CurrentEnvironment property of the test class.
            // This is useful to access the database connection for example.
            HttpClient client = CurrentEnvironment.GetWebApplication().HttpClient;
            HttpResponseMessage response = await client.PostAsync("users", null);

            // Then assert that the database is in the expected state
            Assert.True(response.IsSuccessStatusCode);

            MySqlServerInfrastructure sqlInfrastructure = CurrentEnvironment.GetInfrastructure<MySqlServerInfrastructure>();
            await using (DbConnection sql = sqlInfrastructure.GetDatabaseConnection())
            {
                // Then act with a call to the API
                await sql.OpenAsync(TestContext.Current.CancellationToken);
                using (DbCommand command = sql.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM Users";
                    int count = (int)await command.ExecuteScalarAsync(TestContext.Current.CancellationToken);
                    Assert.Equal(1, count);
                }
            }
        }
    }
}

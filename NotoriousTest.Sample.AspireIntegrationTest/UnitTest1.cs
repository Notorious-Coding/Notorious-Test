using NotoriousTest.Web;
using NotoriousTest.XUnit;

using System.Data.Common;

namespace NotoriousTest.Sample.AspireIntegrationTest
{
    public class UnitTest1 : IntegrationTest<AspireEnvironment>
    {
        public UnitTest1(AspireEnvironment environment) : base(environment)
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

            SqlServerAspireInfrastructure sqlInfrastructure = CurrentEnvironment.GetInfrastructure<SqlServerAspireInfrastructure>();
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

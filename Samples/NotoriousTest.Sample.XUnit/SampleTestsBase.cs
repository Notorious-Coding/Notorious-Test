using NotoriousTest.Sample.XUnit.Environments;
using NotoriousTest.Sample.XUnit.Infrastructures;
using NotoriousTest.Web;
using NotoriousTest.XUnit;

using System.Data.Common;
using NotoriousTest.Core.DI;
using NotoriousTest.XUnit.DI;

namespace NotoriousTest.Sample.XUnit
{
    public class SampleTestsBase : IntegrationTest<TestEnvironment>
    {
        public SampleTestsBase(XUnitFixture<TestEnvironment> fixture) : base(fixture)
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
            HttpClient client = Environment.GetWebApplication().HttpClient;
            HttpResponseMessage response = await client.PostAsync("users", null);

            // Then assert that the database is in the expected state
            Assert.True(response.IsSuccessStatusCode);

            SqlServerInfrastructure sqlInfrastructure = Environment.GetInfrastructure<SqlServerInfrastructure>();
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
            HttpClient client = Environment.GetWebApplication().HttpClient;
            HttpResponseMessage response = await client.PostAsync("users", null);

            // Then assert that the database is in the expected state
            Assert.True(response.IsSuccessStatusCode);

            SqlServerInfrastructure sqlInfrastructure = Environment.GetInfrastructure<SqlServerInfrastructure>();
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

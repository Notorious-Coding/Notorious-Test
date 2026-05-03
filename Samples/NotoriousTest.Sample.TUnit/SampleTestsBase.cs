using NotoriousTest.Web;

using NotoriousTest.Sample.TUnit.Environments;
using NotoriousTest.Sample.TUnit.Infrastructures;

using System.Data.Common;
using NotoriousTest.TUnit;

namespace NotoriousTest.Sample.TUnit
{
    public class SampleTestsBase : NotoriousTest.TUnit.IntegrationTestBase<TestEnvironment>
    {
        public SampleTestsBase(TUnitFixture<TestEnvironment> fixture) : base(fixture)
        {
            // Do not hesitate to create your test framework for your app, that will use the environment.
            // Example : new MyAppTestFramework(environment);
            // Then, you could create multiple methods to assert, arrange, act
            // that you could do multiple times in your tests.
        }

        [Test]
        public async Task Test1()
        {
            // You can access an infrastructure directly from the CurrentEnvironment property of the test class.
            // This is useful to access the database connection for example.
            HttpClient client = Environment.GetWebApplication().HttpClient;
            HttpResponseMessage response = await client.PostAsync("users", null);

            // Then assert that the database is in the expected state
            await Assert.That(response.IsSuccessStatusCode).IsTrue();

            SqlServerInfrastructure sqlInfrastructure = Environment.GetInfrastructure<SqlServerInfrastructure>();
            await using (DbConnection sql = sqlInfrastructure.GetDatabaseConnection())
            {
                await sql.OpenAsync();
                using (DbCommand command = sql.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM Users";
                    int count = (int)await command.ExecuteScalarAsync();
                    await Assert.That(count).IsEqualTo(count);
                }
            }
        }

        [Test]
        public async Task Test2()
        {
            // You can access an infrastructure directly from the CurrentEnvironment property of the test class.
            // This is useful to access the database connection for example.
            HttpClient client = Environment.GetWebApplication().HttpClient;
            HttpResponseMessage response = await client.PostAsync("users", null);

            // Then assert that the database is in the expected state
            await Assert.That(response.IsSuccessStatusCode).IsTrue();

            SqlServerInfrastructure sqlInfrastructure = Environment.GetInfrastructure<SqlServerInfrastructure>();
            await using (DbConnection sql = sqlInfrastructure.GetDatabaseConnection())
            {
                await sql.OpenAsync();
                using (DbCommand command = sql.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM Users";
                    int count = (int)await command.ExecuteScalarAsync();
                    await Assert.That(count).IsEqualTo(count);
                }
            }
        }
    }
}

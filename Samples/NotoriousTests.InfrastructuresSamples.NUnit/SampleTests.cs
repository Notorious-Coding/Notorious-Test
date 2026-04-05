using NUnit.Framework;

using NotoriousTest.Web;

using NotoriousTests.InfrastructuresSamples.NUnit.Environments;
using NotoriousTests.InfrastructuresSamples.NUnit.Infrastructures;

using System.Data.Common;

namespace NotoriousTests.InfrastructuresSamples.NUnit
{
    [TestFixture]
    public class SampleTests : NotoriousTest.NUnit.IntegrationTest<TestEnvironment>
    {
        // Do not hesitate to create your test framework for your app, that will use the environment.
        // Example : new MyAppTestFramework(environment);
        // Then, you could create multiple methods to assert, arrange, act
        // that you could do multiple times in your tests.

        [Test]
        public async Task Test1()
        {
            // You can access an infrastructure directly from the CurrentEnvironment property of the test class.
            // This is useful to access the database connection for example.
            HttpClient client = CurrentEnvironment.GetWebApplication().HttpClient;
            HttpResponseMessage response = await client.PostAsync("users", null);

            // Then assert that the database is in the expected state
            Assert.That(response.IsSuccessStatusCode, Is.True);

            SqlServerInfrastructure sqlInfrastructure = CurrentEnvironment.GetInfrastructure<SqlServerInfrastructure>();
            await using (DbConnection sql = sqlInfrastructure.GetDatabaseConnection())
            {
                await sql.OpenAsync();
                using (DbCommand command = sql.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM Users";
                    int count = (int)await command.ExecuteScalarAsync();
                    Assert.That(count, Is.EqualTo(1));
                }
            }
        }

        [Test]
        public async Task Test2()
        {
            // You can access an infrastructure directly from the CurrentEnvironment property of the test class.
            // This is useful to access the database connection for example.
            HttpClient client = CurrentEnvironment.GetWebApplication().HttpClient;
            HttpResponseMessage response = await client.PostAsync("users", null);

            // Then assert that the database is in the expected state
            Assert.That(response.IsSuccessStatusCode, Is.True);

            SqlServerInfrastructure sqlInfrastructure = CurrentEnvironment.GetInfrastructure<SqlServerInfrastructure>();
            await using (DbConnection sql = sqlInfrastructure.GetDatabaseConnection())
            {
                await sql.OpenAsync();
                using (DbCommand command = sql.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM Users";
                    int count = (int)await command.ExecuteScalarAsync();
                    Assert.That(count, Is.EqualTo(1));
                }
            }
        }
    }
}

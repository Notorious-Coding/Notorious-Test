using Microsoft.VisualStudio.TestTools.UnitTesting;

using NotoriousTest.Web;

using NotoriousTest.Sample.MSTest.Environments;
using NotoriousTest.Sample.MSTest.Infrastructures;

using System.Data.Common;

namespace NotoriousTest.Sample.MSTest
{
    [TestClass]
    public class SampleTestsBase : NotoriousTest.MSTest.IntegrationTestBase<TestEnvironment>
    {
        // Do not hesitate to create your test framework for your app, that will use the environment.
        // Example : new MyAppTestFramework(environment);
        // Then, you could create multiple methods to assert, arrange, act
        // that you could do multiple times in your tests.

        [TestMethod]
        public async Task Test1()
        {
            // You can access an infrastructure directly from the CurrentEnvironment property of the test class.
            // This is useful to access the database connection for example.
            HttpClient client = Environment.GetWebApplication().HttpClient;
            HttpResponseMessage response = await client.PostAsync("users", null);

            // Then assert that the database is in the expected state
            Assert.IsTrue(response.IsSuccessStatusCode);

            SqlServerInfrastructure sqlInfrastructure = Environment.GetInfrastructure<SqlServerInfrastructure>();
            await using (DbConnection sql = sqlInfrastructure.GetDatabaseConnection())
            {
                await sql.OpenAsync();
                using (DbCommand command = sql.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM Users";
                    int count = (int)await command.ExecuteScalarAsync();
                    Assert.AreEqual(1, count);
                }
            }
        }

        [TestMethod]
        public async Task Test2()
        {
            // You can access an infrastructure directly from the CurrentEnvironment property of the test class.
            // This is useful to access the database connection for example.
            HttpClient client = Environment.GetWebApplication().HttpClient;
            HttpResponseMessage response = await client.PostAsync("users", null);

            // Then assert that the database is in the expected state
            Assert.IsTrue(response.IsSuccessStatusCode);

            SqlServerInfrastructure sqlInfrastructure = Environment.GetInfrastructure<SqlServerInfrastructure>();
            await using (DbConnection sql = sqlInfrastructure.GetDatabaseConnection())
            {
                await sql.OpenAsync();
                using (DbCommand command = sql.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM Users";
                    int count = (int)await command.ExecuteScalarAsync();
                    Assert.AreEqual(1, count);
                }
            }
        }
    }
}

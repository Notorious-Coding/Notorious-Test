using System.Data.Common;
using NotoriousTest.Sample.NUnit.Environments;
using NotoriousTest.Sample.NUnit.Infrastructures;
using NotoriousTest.Web;
using NUnit.Framework;

namespace NotoriousTest.Sample.NUnit;

[TestFixture]
public class SampleTestsBase : NotoriousTest.NUnit.IntegrationTestBase<TestEnvironment>
{
    [Test]
    public async Task Test1()
    {
        // You can access an infrastructure directly from the CurrentEnvironment property of the test class.
        // This is useful to access the database connection for example.
        HttpClient? client = Environment.GetWebApplication().HttpClient;
        HttpResponseMessage response = await client?.PostAsync("users", null)!;

        // Then assert that the database is in the expected state
        Assert.That(response.IsSuccessStatusCode, Is.True);

        SqlServerInfrastructure sqlInfrastructure = Environment.GetInfrastructure<SqlServerInfrastructure>();
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
        HttpClient client = Environment.GetWebApplication().HttpClient;
        HttpResponseMessage response = await client.PostAsync("users", null);

        // Then assert that the database is in the expected state
        Assert.That(response.IsSuccessStatusCode, Is.True);

        SqlServerInfrastructure sqlInfrastructure = Environment.GetInfrastructure<SqlServerInfrastructure>();
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

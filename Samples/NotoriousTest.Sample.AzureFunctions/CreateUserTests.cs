using System.Data.Common;
using System.Net;
using NotoriousTest.Sample.AzureFunctions.Environments;
using NotoriousTest.Sample.AzureFunctions.Infrastructures;
using NotoriousTest.XUnit;

namespace NotoriousTest.Sample.AzureFunctions;

public class CreateUserTests : IntegrationTest<TestEnvironment>
{
    public CreateUserTests(XUnitFixture<TestEnvironment> fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Post_Users_Should_Create_A_User()
    {
        // HttpClient targeting the local Azure Functions host.
        HttpClient client = Environment.GetInfrastructure<FunctionAppInfrastructure>().HttpClient!;

        HttpResponseMessage response = await client.PostAsync("users", null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // The function wrote the user in the database.
        Assert.Equal(1, await CountUsers());
    }

    [Fact]
    public async Task Database_Should_Be_Reset_Between_Tests() =>
        // The SqlServerInfrastructure is reset with Respawn before each test.
        Assert.Equal(0, await CountUsers());

    private async Task<int> CountUsers()
    {
        SqlServerInfrastructure sqlInfrastructure = Environment.GetInfrastructure<SqlServerInfrastructure>();
        await using DbConnection sql = sqlInfrastructure.GetDatabaseConnection();
        await sql.OpenAsync(TestContext.Current.CancellationToken);

        await using DbCommand command = sql.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Users";
        return (int)(await command.ExecuteScalarAsync(TestContext.Current.CancellationToken))!;
    }
}

using System.Data.Common;
using Microsoft.Playwright;
using NotoriousTest.Sample.Playwright.Environments;
using NotoriousTest.Sample.Playwright.Infrastructures;
using NotoriousTest.Web.Playwright;
using NotoriousTest.XUnit;
using static Microsoft.Playwright.Assertions;

namespace NotoriousTest.Sample.Playwright;

public class CreateUserTests : IntegrationTest<TestEnvironment>
{
    public CreateUserTests(XUnitFixture<TestEnvironment> fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Clicking_CreateUser_Should_Create_A_User()
    {
        // The browser is reset before each test: new context, new page.

        // Relative to the front url (BaseURL of the browser context).
        await Environment.Page.GotoAsync("/");
        await Environment.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "CreateUser" })
            .ClickAsync();

        // The front called the API through the Vite proxy.
        await Expect(Environment.Page.GetByRole(AriaRole.Status)).ToHaveTextAsync("User created");

        // The API wrote the user in the database.
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

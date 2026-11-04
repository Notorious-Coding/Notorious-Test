using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace NotoriousTest.Sample.FunctionApp;

public class UserFunctions(IConfiguration configuration)
{
    // In the tests, the connection string is given by the SqlServerInfrastructure,
    // through the ConnectionStrings__SqlServer environment variable of the func process.
    [Function("CreateUser")]
    public async Task<IActionResult> CreateUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "users")] HttpRequest request)
    {
        await using var connection = new SqlConnection(configuration.GetConnectionString("SqlServer"));
        await connection.OpenAsync();

        await using SqlCommand command = connection.CreateCommand();
        string id = Guid.NewGuid().ToString("N")[..8];
        command.Parameters.AddWithValue("@username", $"user{id}");
        command.Parameters.AddWithValue("@email", $"user{id}@example.com");
        command.Parameters.AddWithValue("@password_hash", "password");
        command.CommandText =
            "INSERT INTO Users(username, email, password_hash) VALUES(@username, @email, @password_hash);";
        await command.ExecuteNonQueryAsync();

        return new StatusCodeResult(StatusCodes.Status201Created);
    }
}

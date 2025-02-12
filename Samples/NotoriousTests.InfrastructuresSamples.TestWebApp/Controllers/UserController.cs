using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using System;

namespace NotoriousTests.InfrastructuresSamples.TestWebApp.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UserController(IConfiguration configuration)
        {
            this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpPost]
        public void CreateUser()
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("SqlServer")))
            {
                connection.Open();
                CreateUser(connection);
            }
        }

        private void CreateUser(SqlConnection sqlConnection)
        {
            using (var command = sqlConnection.CreateCommand())
            {
                command.Parameters.AddWithValue("@username", "test");
                command.Parameters.AddWithValue("@email", "example@email.com");
                command.Parameters.AddWithValue("@password_hash", "password");
                command.Parameters.AddWithValue("@created_at", DateTime.Now);
                command.CommandText = "INSERT INTO Users(username, email, password_hash, created_at) VALUES(@username, @email, @password_hash, @created_at);";
                command.ExecuteNonQuery();
            }
        }
    }
}

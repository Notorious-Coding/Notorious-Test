using System.Data.Common;

using Xunit;

namespace NotoriousTest.Sample.Without
{
    public class UnitTest1 : IClassFixture<SystemUnderTest>, IAsyncLifetime
    {

        public UnitTest1(SystemUnderTest sut)
        {
            Sut = sut;
        }

        public SystemUnderTest Sut { get; }

        public async ValueTask DisposeAsync()
        {
            using var connection = Sut.GetDatabaseConnection();
            await connection.OpenAsync();
            await Sut.Respawner.ResetAsync(connection);
        }

        public async ValueTask InitializeAsync()
        {
        }

        [Fact]
        public async Task Test1()
        {
            // You can access an infrastructure directly from the CurrentEnvironment property of the test class.
            // This is useful to access the database connection for example.
            HttpClient client = Sut.Client;
            HttpResponseMessage response = await client.PostAsync("users", null);

            // Then assert that the database is in the expected state
            Assert.True(response.IsSuccessStatusCode);

            await using (DbConnection sql = Sut.GetDatabaseConnection())
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
            HttpClient client = Sut.Client;
            HttpResponseMessage response = await client.PostAsync("users", null);

            // Then assert that the database is in the expected state
            Assert.True(response.IsSuccessStatusCode);

            await using (DbConnection sql = Sut.GetDatabaseConnection())
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

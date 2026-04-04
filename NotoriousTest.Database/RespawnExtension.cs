using Respawn;

namespace NotoriousTest.Database
{
    public class RespawnExtension : IInfrastructureExtension<IDatabaseInfrastructure>
    {
        private readonly Func<RespawnerOptions>? _optionsFactory;
        RespawnerOptions? _options => _optionsFactory?.Invoke();
        private Respawner? _respawner;

        public RespawnExtension(Func<RespawnerOptions>? optionsFactory = null)
        {
            _optionsFactory = optionsFactory;
        }

        public async Task OnBeforeReset(IDatabaseInfrastructure infra)
        {
            try
            {
                using var connection = infra.GetDatabaseConnection();
                await connection.OpenAsync();
                if (_respawner == null) _respawner = await Respawner.CreateAsync(connection, _options);
                await _respawner.ResetAsync(connection);
            }
            catch (InvalidOperationException exception)
            {
                // This can occur if the database has no tables. In that case, we can ignore the exception and continue with the test setup.
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to reset database using Respawn.", ex);
            }
        }
    }
}
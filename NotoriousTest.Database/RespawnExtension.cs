using Respawn;

namespace NotoriousTest.Database
{
    public class RespawnExtension : IInfrastructureExtension<IDatabaseInfrastructure>
    {
        private readonly Func<RespawnerOptions>? _optionsFactory;
        private Respawner _respawner;

        public RespawnExtension(Func<RespawnerOptions>? optionsFactory = null)
        {
            _optionsFactory = optionsFactory;
        }

        public async Task OnAfterInitialize(IDatabaseInfrastructure infra)
        {
            using var connection = infra.GetDatabaseConnection();
            await connection.OpenAsync();
            _respawner = await Respawner.CreateAsync(connection, _optionsFactory?.Invoke());
        }

        public async Task OnBeforeReset(IDatabaseInfrastructure infra)
        {
            using var connection = infra.GetDatabaseConnection();
            await connection.OpenAsync();
            await _respawner.ResetAsync(connection);
        }
    }
}
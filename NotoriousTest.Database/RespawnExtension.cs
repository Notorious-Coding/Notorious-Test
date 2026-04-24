using Respawn;

namespace NotoriousTest.Database
{
    /// <summary>
    /// Infrastructure extension that uses Respawn to reset the database to a clean state between tests.
    /// </summary>
    public class RespawnExtension : IInfrastructureExtension<IDatabaseInfrastructure>
    {
        private readonly Func<RespawnerOptions>? _optionsFactory;
        private Respawner? _respawner;

        /// <summary>Initializes a new instance with an optional Respawn options factory.</summary>
        /// <param name="optionsFactory">Factory returning the Respawn options, or null to use defaults.</param>
        public RespawnExtension(Func<RespawnerOptions>? optionsFactory = null)
        {
            _optionsFactory = optionsFactory;
        }

        /// <summary>Creates the Respawner after the database infrastructure is initialized.</summary>
        public async Task OnAfterInitialize(IDatabaseInfrastructure infra)
        {
            using var connection = infra.GetDatabaseConnection();
            await connection.OpenAsync();
            _respawner = await Respawner.CreateAsync(connection, _optionsFactory?.Invoke());
        }

        /// <summary>Resets the database to the checkpoint state before each test reset.</summary>
        public async Task OnBeforeReset(IDatabaseInfrastructure infra)
        {
            using var connection = infra.GetDatabaseConnection();
            await connection.OpenAsync();
            await _respawner!.ResetAsync(connection);
        }
    }
}

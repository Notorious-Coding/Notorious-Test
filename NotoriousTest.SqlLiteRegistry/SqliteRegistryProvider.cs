using Dapper;

using Microsoft.Data.Sqlite;

using NotoriousTest.Core;
using NotoriousTest.Core.Registry;
namespace NotoriousTest.SqlLiteRegistry
{
    internal partial class SqliteRegistryProvider : IRegistry, IAsyncDisposable
    {
        private readonly SqliteRegistryProviderConfiguration _configuration;
        public event Action<InfrastuctureRegistryEntry> OnInfrastructureReset;
        public event Action<InfrastuctureRegistryEntry> OnInfrastructureDestroyed;
        public event Action<InfrastuctureRegistryEntry> OnInfrastructureCreated;

        public SqliteRegistryProvider(SqliteRegistryProviderConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqliteConnection _connection;
        private SqliteConnectionStringBuilder ConnectionString => new SqliteConnectionStringBuilder(_configuration.ConnectionString);
        protected SqliteConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = new SqliteConnection(ConnectionString.ToString());
                }

                if (_connection.State != System.Data.ConnectionState.Open)
                {
                    _connection.Open();
                }

                _connection.Execute("PRAGMA SYNCHRONOUS=NORMAL;PRAGMA JOURNAL_MODE=WAL;");
                return _connection;
            }
        }


        public async Task Ensure()
        {
            string directory = Path.GetDirectoryName(ConnectionString.DataSource);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            await Connection.ExecuteAsync(ENSURE_REGISTRY);
        }

        public async Task<InfrastuctureRegistryEntry> Register(InfrastuctureRegistryEntry entry)
        {

            InfrastructureRegistryEntryEntity entity = await Connection.QuerySingleAsync<InfrastructureRegistryEntryEntity>(REGISTER_INFRASTRUCTURE, InfrastructureRegistryEntryEntity.FromDomain(entry));

            return entity.ToDomain();
        }

        public async ValueTask DisposeAsync()
        {
            await Connection.DisposeAsync();
        }

        public async Task<bool> Remove(Guid id)
        {
            int deletedRows = await Connection.ExecuteAsync(REMOVE_INFRASTRUCTURE, new { InfrastructureId = id.ToString() });
            return deletedRows > 0;
        }

        public async Task NotifyReset(Guid id)
        {
            await Connection.ExecuteAsync(UPDATE_INFRASTRUCTURE_RESET_DATE, new { InfrastructureId = id.ToString() });
        }

        public async Task<IEnumerable<InfrastuctureRegistryEntry>> GetByProcessId(int processId)
        {
            IEnumerable<InfrastructureRegistryEntryEntity> entries = await Connection.QueryAsync<InfrastructureRegistryEntryEntity>(GET_BY_PROCESS_ID, new { ProcessID = processId });
            return entries.Select(entry => entry.ToDomain());
        }

        public async Task<IEnumerable<InfrastuctureRegistryEntry>> GetByEnvironmentId(EnvironmentId environmentId)
        {
            IEnumerable<InfrastructureRegistryEntryEntity> entries = await Connection.QueryAsync<InfrastructureRegistryEntryEntity>(GET_BY_ENVIRONMENT_ID, new { EnvironmentId = environmentId.Value.ToString() });
            return entries.Select(entry => entry.ToDomain());
        }

        public async Task Watch(EnvironmentId environmentId, CancellationToken ct)
        {
            Dictionary<Guid, InfrastuctureRegistryEntry> snapshot = (await GetByEnvironmentId(environmentId)).ToDictionary(x => x.InfrastructureId);

            do
            {
                IEnumerable<InfrastuctureRegistryEntry> infras = await GetByEnvironmentId(environmentId);

                foreach (InfrastuctureRegistryEntry infra in infras)
                {
                    if (!snapshot.TryGetValue(infra.InfrastructureId, out var previous))
                    {
                        OnInfrastructureCreated?.Invoke(infra);
                    }

                    if (previous is not null && infra.LastResetDate != previous.LastResetDate)
                    {
                        OnInfrastructureReset?.Invoke(infra);
                    }
                }

                foreach (var (key, infrastructure) in snapshot)
                {
                    if (!infras.Any(i => i.InfrastructureId == infrastructure.InfrastructureId))
                    {
                        OnInfrastructureDestroyed?.Invoke(infrastructure);
                    }
                }

                snapshot = infras.ToDictionary(x => x.InfrastructureId);

                try
                {
                    await Task.Delay(10, ct);
                }
                catch (OperationCanceledException) { }
            } while (!ct.IsCancellationRequested);
        }
    }
}

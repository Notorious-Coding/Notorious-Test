namespace NotoriousTest.SqlLiteRegistry
{
    public static class SqliteRegistryProviderQueries
    {
        public const string CREATE_REGISTRY = @"
            CREATE TABLE IF NOT EXISTS InfrastructureRegistry(
                InfrastructureId TEXT PRIMARY KEY,
                InfrastructureType TEXT NOT NULL,
                EnvironmentId TEXT NOT NULL,
                ProcessPID TEXT NOT NULL,
                Metadata TEXT,
                CreationDate TEXT NOT NULL,
                UpdateDate TEXT NOT NULL
            )
        ";

        public const string REGISTER_INFRASTRUCTURE = @"
            INSERT INTO InfrastructureRegistry(InfrastructureId, InfrastructureType, EnvironmentId, ProcessPID, Metadata, CreationDate, UpdateDate)
            VALUES(@InfrastructureId, @InfrastructureType, @EnvironmentId, @ProcessPID, @Metadata, datetime('now', 'utc'), datetime('now', 'utc'))
            RETURNING *
        ";
    }
}

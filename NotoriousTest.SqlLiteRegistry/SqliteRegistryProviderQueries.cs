namespace NotoriousTest.SqlLiteRegistry
{
    public static class SqliteRegistryProviderQueries
    {
        public const string ENSURE_REGISTRY = @"
            CREATE TABLE IF NOT EXISTS InfrastructureRegistry(
                InfrastructureId TEXT PRIMARY KEY,
                InfrastructureType TEXT NOT NULL,
                EnvironmentId TEXT NOT NULL,
                ProcessPID TEXT NOT NULL,
                Metadata TEXT,
                MetadataType TEXT,
                CreationDate TEXT NOT NULL,
                UpdateDate TEXT NOT NULL
            )
        ";

        public const string REGISTER_INFRASTRUCTURE = @"
            INSERT INTO InfrastructureRegistry(InfrastructureId, InfrastructureType, EnvironmentId, ProcessPID, Metadata, MetadataType, CreationDate, UpdateDate)
            VALUES(@InfrastructureId, @InfrastructureType, @EnvironmentId, @ProcessPID, @Metadata, @MetadataType, datetime('now', 'utc'), datetime('now', 'utc'))
            RETURNING *
        ";

        public const string REMOVE_INFRASTRUCTURE = @"
            DELETE FROM InfrastructureRegistry WHERE InfrastructureId = @InfrastructureId;
        ";
    }
}

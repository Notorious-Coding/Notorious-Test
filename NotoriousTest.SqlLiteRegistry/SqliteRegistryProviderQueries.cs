namespace NotoriousTest.SqlLiteRegistry
{
    public partial class SqliteRegistryProvider
    {
        public const string ENSURE_REGISTRY = @"
            CREATE TABLE IF NOT EXISTS InfrastructureRegistry(
                InfrastructureId TEXT PRIMARY KEY,
                InfrastructureType TEXT NOT NULL,
                EnvironmentId TEXT NOT NULL,
                ProcessID TEXT NOT NULL,
                Metadata TEXT,
                MetadataType TEXT,
                CreationDate TEXT NOT NULL,
                UpdateDate TEXT NOT NULL
            )
        ";

        public const string REGISTER_INFRASTRUCTURE = @"
            INSERT INTO InfrastructureRegistry(InfrastructureId, InfrastructureType, EnvironmentId, ProcessID, Metadata, MetadataType, CreationDate, UpdateDate)
            VALUES(@InfrastructureId, @InfrastructureType, @EnvironmentId, @ProcessID, @Metadata, @MetadataType, datetime('now', 'utc'), datetime('now', 'utc'))
            RETURNING *
        ";

        public const string REMOVE_INFRASTRUCTURE = @"
            DELETE FROM InfrastructureRegistry WHERE InfrastructureId = @InfrastructureId;
        ";

        public const string GET_BY_PROCESS_ID = @"
            SELECT * FROM InfrastructureRegistry WHERE ProcessID = @ProcessID;
        ";

        public const string GET_BY_ENVIRONMENT_ID = @"
            SELECT * FROM InfrastructureRegistry WHERE EnvironmentId = @EnvironmentId;
        ";
    }
}

namespace NotoriousTest.SqlLiteRegistry
{
    internal partial class SqliteRegistryRepository
    {
        protected const string TABLE_NAME = "InfrastructureRegistry";
        private const string VERSION = "2";
        private const string VERSIONED_TABLE_NAME = $"{TABLE_NAME}V{VERSION}";

        private const string ENSURE_REGISTRY = @$"
            CREATE TABLE IF NOT EXISTS {VERSIONED_TABLE_NAME}(
                InfrastructureId TEXT PRIMARY KEY,
                InfrastructureType TEXT NOT NULL,
                EnvironmentId TEXT NOT NULL,
                ProcessID TEXT NOT NULL,
                ProcessName TEXT NOT NULL,
                Metadata TEXT,
                MetadataType TEXT,
                CreationDate TEXT NOT NULL,
                UpdateDate TEXT NOT NULL,
                LastResetDate TEXT
            )
        ";

        private const string REGISTER_INFRASTRUCTURE = @$"
            INSERT INTO {VERSIONED_TABLE_NAME}(InfrastructureId, InfrastructureType, EnvironmentId, ProcessID, ProcessName, Metadata, MetadataType, CreationDate, UpdateDate, LastResetDate)
            VALUES(@InfrastructureId, @InfrastructureType, @EnvironmentId, @ProcessID, @ProcessName, @Metadata, @MetadataType, datetime('now', 'utc'), datetime('now', 'utc'), null)
            RETURNING *
        ";

        private const string REMOVE_INFRASTRUCTURE = @$"
            DELETE FROM {VERSIONED_TABLE_NAME} WHERE InfrastructureId = @InfrastructureId;
        ";

        private const string GET_BY_PROCESS_ID = @$"
            SELECT * FROM {VERSIONED_TABLE_NAME} WHERE ProcessID = @ProcessID;
        ";

        private const string GET_BY_ENVIRONMENT_ID = @$"
            SELECT * FROM {VERSIONED_TABLE_NAME} WHERE EnvironmentId = @EnvironmentId;
        ";

        protected const string GET_BY_ROWID = $@"
            SELECT * FROM {VERSIONED_TABLE_NAME} WHERE rowid = @rowId
        ";

        private const string UPDATE_INFRASTRUCTURE_RESET_DATE = @$"
            UPDATE {VERSIONED_TABLE_NAME} SET LastResetDate = strftime('%Y-%m-%dT%H:%M:%f', 'now') WHERE InfrastructureId = @InfrastructureId
        ";
    }
}

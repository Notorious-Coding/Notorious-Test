namespace NotoriousTest.Internal.SqlLiteRegistry;

internal partial class SqliteRegistryProvider
{
    protected const string TABLE_NAME = "InfrastructureRegistry";

    protected const string ENSURE_REGISTRY = @$"
            CREATE TABLE IF NOT EXISTS {TABLE_NAME}(
                InfrastructureId TEXT PRIMARY KEY,
                InfrastructureType TEXT NOT NULL,
                EnvironmentId TEXT NOT NULL,
                ProcessID TEXT NOT NULL,
                Metadata TEXT,
                MetadataType TEXT,
                CreationDate TEXT NOT NULL,
                UpdateDate TEXT NOT NULL,
                LastResetDate TEXT
            )
        ";

    protected const string REGISTER_INFRASTRUCTURE = @$"
            INSERT INTO {TABLE_NAME}(InfrastructureId, InfrastructureType, EnvironmentId, ProcessID, Metadata, MetadataType, CreationDate, UpdateDate, LastResetDate)
            VALUES(@InfrastructureId, @InfrastructureType, @EnvironmentId, @ProcessID, @Metadata, @MetadataType, datetime('now', 'utc'), datetime('now', 'utc'), null)
            RETURNING *
        ";

    protected const string REMOVE_INFRASTRUCTURE = @$"
            DELETE FROM {TABLE_NAME} WHERE InfrastructureId = @InfrastructureId;
        ";

    protected const string GET_BY_PROCESS_ID = @$"
            SELECT * FROM {TABLE_NAME} WHERE ProcessID = @ProcessID;
        ";

    protected const string GET_BY_ENVIRONMENT_ID = @$"
            SELECT * FROM {TABLE_NAME} WHERE EnvironmentId = @EnvironmentId;
        ";

    protected const string GET_BY_ROWID = $@"
            SELECT * FROM {TABLE_NAME} WHERE rowid = @rowId
        ";

    protected const string UPDATE_INFRASTRUCTURE_RESET_DATE = @$"
            UPDATE {TABLE_NAME} SET LastResetDate = strftime('%Y-%m-%dT%H:%M:%f', 'now') WHERE InfrastructureId = @InfrastructureId
        ";
}

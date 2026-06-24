#!/usr/bin/dotnet run
#:package Dapper
#:package Microsoft.Data.Sqlite
Console.WriteLine("Hello World!");
const string REGISTER_INFRASTRUCTURE = @"
            INSERT INTO InfrastructureRegistryV2(InfrastructureId, InfrastructureType, EnvironmentId, ProcessID, ProcessName, Metadata, MetadataType, CreationDate, UpdateDate, LastResetDate)
            VALUES(@InfrastructureId, @InfrastructureType, @EnvironmentId, @ProcessID, @ProcessName, @Metadata, @MetadataType, datetime('now', 'utc'), datetime('now', 'utc'), null)
            RETURNING *
        ";

string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "notorioustest/doggydog-registry.db");
var connection = new SqliteConnection($"Data Source={path};");
await connection.OpenAsync();

var entries = new List<InfrastructureRegistryEntry>
{
    // ── Process 1 : MyProject.IntegrationTests (PID 12345) ──────────────────
    //    Environment A
    new()
    {
        InfrastructureId = "a1b2c3d4-0000-0000-0000-000000000001",
        InfrastructureType = "NotoriousTest.TestContainers.DockerInfrastructure",
        EnvironmentId = "eeee0001-0000-0000-0000-000000000000",
        ProcessID = 12345,
        ProcessName = "MyProject.IntegrationTests",
        Metadata = """{"ContainerId":"abc123def456","ContainerName":"postgres-test-01"}""",
        MetadataType = "NotoriousTest.TestContainers.DockerMetadata",
        LastResetDate = null,
        CreationDate = "2026-05-12T13:21:05Z",
        UpdateDate = "2026-05-12T13:21:05Z"
    },
    new()
    {
        InfrastructureId = "a1b2c3d4-0000-0000-0000-000000000002",
        InfrastructureType = "NotoriousTest.Database.DatabaseInfrastructure",
        EnvironmentId = "eeee0001-0000-0000-0000-000000000000",
        ProcessID = 12345,
        ProcessName = "MyProject.IntegrationTests",
        Metadata =
            """{"ConnectionString":"Server=localhost;Port=5432;Database=test_db;User Id=sa;Password=pass;"}""",
        MetadataType = "NotoriousTest.Database.DatabaseMetadata",
        LastResetDate = "2026-05-12T13:38:22Z",
        CreationDate = "2026-05-12T13:21:05Z",
        UpdateDate = "2026-05-12T13:38:22Z"
    },

    //    Environment B (même process, env différent)
    new()
    {
        InfrastructureId = "a1b2c3d4-0000-0000-0000-000000000003",
        InfrastructureType = "NotoriousTest.TestContainers.DockerInfrastructure",
        EnvironmentId = "eeee0002-0000-0000-0000-000000000000",
        ProcessID = 12345,
        ProcessName = "MyProject.IntegrationTests",
        Metadata = """{"ContainerId":"ff00ee112233","ContainerName":"redis-cache-01"}""",
        MetadataType = "NotoriousTest.TestContainers.DockerMetadata",
        LastResetDate = "2026-05-12T13:55:00Z",
        CreationDate = "2026-05-12T13:21:05Z",
        UpdateDate = "2026-05-12T13:55:00Z"
    },
    new()
    {
        InfrastructureId = "a1b2c3d4-0000-0000-0000-000000000004",
        InfrastructureType = "NotoriousTest.Database.DatabaseInfrastructure",
        EnvironmentId = "eeee0002-0000-0000-0000-000000000000",
        ProcessID = 12345,
        ProcessName = "MyProject.IntegrationTests",
        Metadata =
            """{"ConnectionString":"Server=localhost;Port=3306;Database=mydb;User Id=root;Password=root;"}""",
        MetadataType = "NotoriousTest.Database.DatabaseMetadata",
        LastResetDate = null,
        CreationDate = "2026-05-12T13:21:05Z",
        UpdateDate = "2026-05-12T13:21:05Z"
    },

    // ── Process 2 : Other.Tests (PID 67890) ─────────────────────────────────
    //    Environment C
    new()
    {
        InfrastructureId = "b9b9b9b9-0000-0000-0000-000000000001",
        InfrastructureType = "NotoriousTest.TestContainers.DockerInfrastructure",
        EnvironmentId = "eeee0003-0000-0000-0000-000000000000",
        ProcessID = 67890,
        ProcessName = "Other.Tests",
        Metadata = """{"ContainerId":"dead0000beef","ContainerName":"sqlserver-test-01"}""",
        MetadataType = "NotoriousTest.TestContainers.DockerMetadata",
        LastResetDate = null,
        CreationDate = "2026-05-12T09:00:00Z",
        UpdateDate = "2026-05-12T09:00:00Z"
    },
    new()
    {
        InfrastructureId = "b9b9b9b9-0000-0000-0000-000000000002",
        InfrastructureType = "NotoriousTest.Database.DatabaseInfrastructure",
        EnvironmentId = "eeee0003-0000-0000-0000-000000000000",
        ProcessID = 67890,
        ProcessName = "Other.Tests",
        Metadata =
            """{"ConnectionString":"Server=localhost;Port=1433;Database=other_db;User Id=sa;Password=P@ssw0rd;"}""",
        MetadataType = "NotoriousTest.Database.DatabaseMetadata",
        LastResetDate = "2026-05-12T09:45:10Z",
        CreationDate = "2026-05-12T09:00:00Z",
        UpdateDate = "2026-05-12T09:45:10Z"
    },
    new()
    {
        InfrastructureId = "b9b9b9b9-0000-0000-0000-000000000003",
        InfrastructureType = "NotoriousTest.TestContainers.DockerInfrastructure",
        EnvironmentId = "eeee0003-0000-0000-0000-000000000000",
        ProcessID = 67890,
        ProcessName = "Other.Tests",
        Metadata = """{"ContainerId":"cafe4242caf3","ContainerName":"rabbitmq-test-01"}""",
        MetadataType = "NotoriousTest.TestContainers.DockerMetadata",
        LastResetDate = null,
        CreationDate = "2026-05-12T09:00:00Z",
        UpdateDate = "2026-05-12T09:00:00Z"
    },

    //    Environment D (même process, env différent, infra sans metadata)
    new()
    {
        InfrastructureId = "b9b9b9b9-0000-0000-0000-000000000004",
        InfrastructureType = "NotoriousTest.Core.Infrastructures.Infrastructure",
        EnvironmentId = "eeee0004-0000-0000-0000-000000000000",
        ProcessID = 67890,
        ProcessName = "Other.Tests",
        Metadata = null,
        MetadataType = null,
        LastResetDate = null,
        CreationDate = "2026-05-12T10:15:00Z",
        UpdateDate = "2026-05-12T10:15:00Z"
    },

    // ── Process 3 : Legacy.EndToEnd.Tests (PID 99999) ───────────────────────
    //    Un seul env, une seule infra, reset multiple fois
    new()
    {
        InfrastructureId = "cccccccc-0000-0000-0000-000000000001",
        InfrastructureType = "NotoriousTest.Database.DatabaseInfrastructure",
        EnvironmentId = "eeee0005-0000-0000-0000-000000000000",
        ProcessID = 99999,
        ProcessName = "Legacy.EndToEnd.Tests",
        Metadata =
            """{"ConnectionString":"Server=192.168.1.50;Port=5432;Database=legacy;User Id=admin;Password=admin;"}""",
        MetadataType = "NotoriousTest.Database.DatabaseMetadata",
        LastResetDate = "2026-05-12T14:01:33Z",
        CreationDate = "2026-05-12T08:00:00Z",
        UpdateDate = "2026-05-12T14:01:33Z"
    }
};

foreach (InfrastructureRegistryEntry entry in entries)
{
    Console.WriteLine(
        $"Insertion of {entry.InfrastructureId} : {entry.ProcessName} ({entry.ProcessName} - {entry.EnvironmentId})");

    var p = new DynamicParameters();
    p.Add("InfrastructureId", entry.InfrastructureId);
    p.Add("InfrastructureType", entry.InfrastructureType);
    p.Add("EnvironmentId", entry.EnvironmentId);
    p.Add("ProcessID", entry.ProcessID);
    p.Add("ProcessName", entry.ProcessName);
    p.Add("Metadata", entry.Metadata);
    p.Add("MetadataType", entry.MetadataType);
    p.Add("LastResetDate", entry.LastResetDate);
    p.Add("CreationDate", entry.CreationDate);
    p.Add("UpdateDate", entry.UpdateDate);

    connection.Execute(REGISTER_INFRASTRUCTURE, p);
}

internal class InfrastructureRegistryEntry
{
    public string InfrastructureId { get; set; }
    public string InfrastructureType { get; set; }
    public string EnvironmentId { get; set; }
    public int ProcessID { get; set; }
    public string ProcessName { get; set; }
    public string? Metadata { get; set; }
    public string? MetadataType { get; set; }
    public string? LastResetDate { get; set; }
    public string CreationDate { get; set; }
    public string UpdateDate { get; set; }
}

using System.Globalization;
using System.Text.Json;
using NotoriousTest.Core.Registry;

namespace NotoriousTest.Internal.SqlLiteRegistry;

internal class InfrastructureRegistryEntryEntity
{
    public string InfrastructureId { get; set; }
    public string InfrastructureType { get; set; }
    public string EnvironmentId { get; set; }
    public int ProcessID { get; set; }
    public string? Metadata { get; set; }
    public string? MetadataType { get; set; }
    public string? LastResetDate { get; set; }
    public string CreationDate { get; set; }
    public string UpdateDate { get; set; }

    internal static InfrastructureRegistryEntryEntity? FromDomain(InfrastuctureRegistryEntry entry) =>
        new()
        {
            InfrastructureId = entry.InfrastructureId.ToString(),
            InfrastructureType = entry.InfrastructureType.AssemblyQualifiedName,
            EnvironmentId = entry.EnvironmentId.ToString(),
            ProcessID = entry.ProcessID,
            Metadata = JsonSerializer.Serialize(entry.Metadata),
            MetadataType = entry.Metadata?.GetType().AssemblyQualifiedName,
            LastResetDate = entry.LastResetDate?.ToString("O")
        };

    internal InfrastuctureRegistryEntry ToDomain()
    {
        Type? metadataType = null;

        if (MetadataType != null) metadataType = Type.GetType(MetadataType);

        var infraType = Type.GetType(InfrastructureType);
        if (infraType == null) throw new TypeLoadException($"Unable to load type {InfrastructureType}");

        return new InfrastuctureRegistryEntry
        {
            InfrastructureId = Guid.Parse(InfrastructureId),
            InfrastructureType = Type.GetType(InfrastructureType),
            EnvironmentId = Guid.Parse(EnvironmentId),
            ProcessID = ProcessID,
            Metadata = !(Metadata is null) && !(metadataType is null)
                ? JsonSerializer.Deserialize(Metadata, metadataType)
                : null,
            LastResetDate =
                LastResetDate != null ? DateTime.Parse(LastResetDate, null, DateTimeStyles.RoundtripKind) : null,
            CreationDate = DateTime.Parse(CreationDate, null, DateTimeStyles.RoundtripKind),
            UpdateDate = DateTime.Parse(UpdateDate, null, DateTimeStyles.RoundtripKind)
        };
    }
}

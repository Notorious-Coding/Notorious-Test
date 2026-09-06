namespace NotoriousTest.Core.Registry;

public class InfrastuctureRegistryEntry
{
    public Guid InfrastructureId { get; init; }
    public Type InfrastructureType { get; init; }
    public Guid EnvironmentId { get; init; }
    public int ProcessID { get; init; }
    public object? Metadata { get; init; }
    public DateTime? LastResetDate { get; init; }
    public DateTime CreationDate { get; init; }
    public DateTime UpdateDate { get; init; }
}

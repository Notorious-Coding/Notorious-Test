namespace NotoriousTest.Core.Registry
{
    /// <summary>
    /// Interface used to access infrastructure registry. Used to track currently started infrastructure.
    /// </summary>
    public interface IRegistry
    {
        event Action<InfrastuctureRegistryEntry> OnInfrastructureCreated;
        event Action<InfrastuctureRegistryEntry> OnInfrastructureDestroyed;
        event Action<InfrastuctureRegistryEntry> OnInfrastructureReset;

        Task Ensure();
        Task<InfrastuctureRegistryEntry> Register(InfrastuctureRegistryEntry entry);
        Task<bool> Remove(Guid id);
        Task<IEnumerable<InfrastuctureRegistryEntry>> GetByProcessId(int processId);
        Task<IEnumerable<InfrastuctureRegistryEntry>> GetByEnvironmentId(EnvironmentId environmentId);
        Task NotifyReset(Guid id);
        Task Watch(EnvironmentId environmentId, CancellationToken ct);
    }
}

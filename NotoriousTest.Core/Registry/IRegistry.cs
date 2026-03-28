namespace NotoriousTest.Core.Registry
{
    /// <summary>
    /// Interface used to access infrastructure registry. Used to track currently started infrastructure.
    /// </summary>
    public interface IRegistry
    {
        Task Ensure();
        Task<InfrastuctureRegistryEntry> Register(InfrastuctureRegistryEntry entry);
        Task<bool> Remove(Guid id);
        Task<IEnumerable<InfrastuctureRegistryEntry>> GetByProcessId(int processId);
    }
}

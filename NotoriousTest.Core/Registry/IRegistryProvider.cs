namespace NotoriousTest.Core.Registry
{
    public interface IRegistryProvider
    {
        Task Ensure();
        Task<InfrastuctureRegistryEntry> Register(InfrastuctureRegistryEntry entry);
    }
}

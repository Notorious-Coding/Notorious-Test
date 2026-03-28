namespace NotoriousTest.Core.Infrastructures.Cleaner
{
    public interface IInfrastructureCleaner<in TMetadata> where TMetadata : class
    {
        Task CleanAfterCrash(ContextId contextId, Guid infrastructureId, TMetadata metadata = null);
    }
}

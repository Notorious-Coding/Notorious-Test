namespace NotoriousTest.Core.Infrastructures.Cleaner
{
    public interface IInfrastructureCleaner<in TMetadata> : IInfrastructureCleaner where TMetadata : class
    {
        Task CleanAfterCrash(EnvironmentId contextId, Guid infrastructureId, TMetadata? metadata = null);
        Task IInfrastructureCleaner.CleanAfterCrash(EnvironmentId contextId, Guid infrastructureId, object? metadata = null) => CleanAfterCrash(contextId, infrastructureId, metadata as TMetadata);
    }

    public interface IInfrastructureCleaner
    {
        Task CleanAfterCrash(EnvironmentId contextId, Guid infrastructureId, object? metadata = null);
    }
}

using Docker.DotNet;

using NotoriousTest.Core;
using NotoriousTest.Core.Infrastructures.Cleaner;
using NotoriousTest.Database;

namespace NotoriousTest.TestContainers
{
    [InfrastructureCleaner(typeof(DockerDatabaseInfrastructure<,>))]
    public class DockerDatabaseInfrastructureCleaner : IInfrastructureCleaner<DockerMetadata>
    {
        public async Task CleanAfterCrash(ContextId contextId, Guid infrastructureId, DockerMetadata? metadata = null)
        {
            var client = new DockerClientConfiguration().CreateClient();
            await client.Containers.StopContainerAsync(metadata.ContainerID.ToString(), new Docker.DotNet.Models.ContainerStopParameters());
        }
    }
}

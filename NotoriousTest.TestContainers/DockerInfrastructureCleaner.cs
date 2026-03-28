using Docker.DotNet;

using NotoriousTest.Core;
using NotoriousTest.Core.Infrastructures.Cleaner;

namespace NotoriousTest.TestContainers
{
    public class DockerInfrastructureCleaner : IInfrastructureCleaner<DockerMetadata>
    {
        public async Task CleanAfterCrash(ContextId contextId, Guid infrastructureId, DockerMetadata? metadata = null)
        {
            var client = new DockerClientConfiguration().CreateClient();
            await client.Containers.StopContainerAsync(metadata.ContainerID, new Docker.DotNet.Models.ContainerStopParameters());
        }
    }
}

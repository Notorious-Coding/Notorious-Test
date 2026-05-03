using Docker.DotNet;
using Docker.DotNet.Models;

using NotoriousTest.Core;
using NotoriousTest.Core.Infrastructures.Cleaner;

namespace NotoriousTest.TestContainers
{
    public class DockerInfrastructureCleaner : IInfrastructureCleaner<DockerMetadata>
    {
        public async Task CleanAfterCrash(EnvironmentId contextId, Guid infrastructureId, DockerMetadata? metadata = null)
        {
            DockerClient? client = new DockerClientConfiguration().CreateClient();
            await client.Containers.RemoveContainerAsync(metadata.ContainerID, new ContainerRemoveParameters
            {
                Force = true
            });
        }
    }
}

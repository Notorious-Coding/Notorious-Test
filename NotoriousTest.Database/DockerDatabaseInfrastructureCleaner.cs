using Docker.DotNet;

using NotoriousTest.Core;
using NotoriousTest.Core.Infrastructures.Cleaner;
using NotoriousTest.Database;

using System.Text.Json;

namespace NotoriousTest.TestContainers
{
    [Cleaner(typeof(DockerDatabaseInfrastructure<,>))]
    public class DockerDatabaseInfrastructureCleaner : IInfrastructureCleaner<DockerMetadata>
    {
        public async Task CleanAfterCrash(ContextId contextId, Guid infrastructureId, DockerMetadata? metadata = null)
        {
            Console.WriteLine(JsonSerializer.Serialize(metadata));
            var client = new DockerClientConfiguration().CreateClient();
            await client.Containers.StopContainerAsync(metadata.ContainerID.ToString(), new Docker.DotNet.Models.ContainerStopParameters());
        }
    }
}

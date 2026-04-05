using Docker.DotNet;

using DotNet.Testcontainers.Containers;

using NotoriousTest.Core;
using NotoriousTest.Core.Infrastructures;
using NotoriousTest.Core.Infrastructures.Cleaner;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;

namespace NotoriousTest.TestContainers
{
    [Cleaner(typeof(DockerInfrastructureCleaner))]
    public class DockerContainerInfrastructure<TContainer, TOutputConfiguration> : Infrastructure<TOutputConfiguration, DockerMetadata>
        where TContainer : IContainer
    {
        public DockerContainerInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry) : base(contextId, logger, registry)
        {
            if (Environment.GetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED") != "true")
                Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");
        }

        public TContainer Container { get; init; }

        public override async Task Initialize()
        {
            await Container.StartAsync();
            Metadata = new DockerMetadata()
            {
                ContainerID = Container.Id,
                ContainerName = Container.Name,
            };
            await Register();
        }
        public override async Task Destroy()
        {
            var client = new DockerClientConfiguration().CreateClient();
            await client.Containers.RemoveContainerAsync(Container.Id, new Docker.DotNet.Models.ContainerRemoveParameters()
            {
                Force = true
            });
        }
    }
}

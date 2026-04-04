using Docker.DotNet;

using DotNet.Testcontainers.Containers;

using NotoriousTest.Core;
using NotoriousTest.Core.Infrastructures.Cleaner;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.TestContainers;

using System.Data.Common;

namespace NotoriousTest.Database
{
    [Cleaner(typeof(DockerInfrastructureCleaner))]
    public abstract class DockerDatabaseInfrastructure<TContainer, TOutputConfiguration> : DatabaseInfrastructureBase<TOutputConfiguration, DockerMetadata> where TContainer : IDatabaseContainer
    {
        protected DockerDatabaseInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry) : base(contextId, logger, registry)
        {
            if (Environment.GetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED") != "true")
                Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");
        }

        public TContainer Container { get; init; }

        public override async Task Initialize()
        {
            await Container.StartAsync();
            Metadata = new DockerMetadata
            {
                ContainerID = Container.Id,
                ContainerName = Container.Name,
            };
            await Register();
            await base.Initialize();
        }

        public override async Task Destroy()
        {
            var client = new DockerClientConfiguration().CreateClient();
            await client.Containers.RemoveContainerAsync(Container.Id, new Docker.DotNet.Models.ContainerRemoveParameters()
            {
                Force = true
            });
        }

        public override string GetServerConnectionString()
        {
            return Container.GetConnectionString();
        }

        protected override async Task DropDatabase(DbConnection connection)
        {
            // Dropping the database is unnecessary since the Docker container is destroyed with it.
        }
    }
}

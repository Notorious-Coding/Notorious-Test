using DotNet.Testcontainers.Containers;

using NotoriousTest.Core;
using NotoriousTest.Core.Infrastructures;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;

namespace NotoriousTest.TestContainers
{
    public abstract class DockerContainerInfrastructure<TContainer, TOutputConfiguration> : Infrastructure<TOutputConfiguration>
        where TContainer : IContainer
    {
        protected DockerContainerInfrastructure(ContextId contextId, ITestLogger logger, IRegistryProvider registry) : base(contextId, logger, registry)
        {
        }

        protected TContainer Container { get; init; }

        public override async Task Destroy()
        {
            await Container.StopAsync();
        }

        public override async Task Initialize()
        {
            await Container.StartAsync();
        }

    }
}

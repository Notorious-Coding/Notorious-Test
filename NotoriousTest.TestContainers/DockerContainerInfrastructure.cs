using DotNet.Testcontainers.Containers;

using NotoriousTest.Infrastructures;
using NotoriousTest.Logger;

namespace NotoriousTest.TestContainers
{
    public abstract class DockerContainerInfrastructure<TContainer, TOutputConfiguration> : Infrastructure<TOutputConfiguration>
        where TContainer : IContainer
    {
        protected DockerContainerInfrastructure(ContextId contextId, ITestLogger logger) : base(contextId, logger)
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

using DotNet.Testcontainers.Containers;

using NotoriousTest.Infrastructures.Async;

namespace NotoriousTest.TestContainers
{

    public abstract class DockerContainerAsyncInfrastructure<TContainer> : AsyncInfrastructure where TContainer : IContainer
    {
        protected DockerContainerAsyncInfrastructure(bool initialize = false) : base(initialize) { }

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

using DotNet.Testcontainers.Containers;

using NotoriousTest.Common.Infrastructures;

namespace NotoriousTest.TestContainers
{

    public abstract class DockerContainerInfrastructure<TContainer> : Infrastructure where TContainer : IContainer
    {
        protected DockerContainerInfrastructure(bool initialize = false) : base(initialize) { }

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

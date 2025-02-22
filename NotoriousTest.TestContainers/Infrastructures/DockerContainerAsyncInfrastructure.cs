using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Configuration;
using NotoriousTest.Common.Infrastructures.Async;
using NotoriousTest.Common.Infrastructures.Common;

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

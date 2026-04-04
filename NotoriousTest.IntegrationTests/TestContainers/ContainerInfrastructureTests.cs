using AwesomeAssertions;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

using FakeItEasy;

using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.TestContainers;

namespace NotoriousTest.IntegrationTests.TestContainers
{
    public class ContainerInfrastructureTests
    {

        [Fact]
        public async Task Initialize_Should_CreateContainer()
        {
            EnvironmentId contextId = Guid.NewGuid();
            await using var infrastructure = new ContainerTestInfrastructure(contextId, A.Fake<ITestLogger>(), A.Fake<IRegistry>());

            await infrastructure.InitializeAsync();
            infrastructure.Metadata?.ContainerID.Should().NotBeNullOrEmpty();

            await TestFramework.Assert.ContainerExist(infrastructure.Metadata!.ContainerID!);
        }

        [Fact]
        public async Task Destroy_Should_DeleteContainer()
        {
            EnvironmentId contextId = Guid.NewGuid();
            var infrastructure = new ContainerTestInfrastructure(contextId, A.Fake<ITestLogger>(), A.Fake<IRegistry>());

            await infrastructure.InitializeAsync();
            infrastructure.Metadata?.ContainerID.Should().NotBeNullOrEmpty();

            await TestFramework.Assert.ContainerExist(infrastructure.Metadata!.ContainerID!);

            await infrastructure.DestroyAsync();
            await TestFramework.Assert.ContainerDoesNotExist(infrastructure.Metadata!.ContainerID!);
        }
    }

    class ContainerTestInfrastructure : DockerContainerInfrastructure<IContainer, string>
    {
        public ContainerTestInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry) : base(contextId, logger, registry)
        {
            Container = new ContainerBuilder("alpine")
                .WithCommand("sleep", "infinity")
                .WithName(nameof(ContainerInfrastructureTests))
                .Build();
        }

        public override async Task Reset()
        {
        }
    }
}

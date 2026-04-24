using DotNet.Testcontainers.Containers;

using NotoriousTest.Infrastructures;
using NotoriousTest.Logger;

namespace NotoriousTest.TestContainers
{
    /// <summary>
    /// Base class for infrastructures backed by a Testcontainers Docker container with typed output configuration.
    /// </summary>
    /// <typeparam name="TContainer">The Testcontainers container type.</typeparam>
    /// <typeparam name="TOutputConfiguration">The type of configuration value produced by this infrastructure.</typeparam>
    public abstract class DockerContainerInfrastructure<TContainer, TOutputConfiguration> : Infrastructure<TOutputConfiguration>
        where TContainer : IContainer
    {
        /// <summary>Initializes a new instance with the given context and logger.</summary>
        /// <param name="contextId">The environment context identifier.</param>
        /// <param name="logger">The test logger.</param>
        protected DockerContainerInfrastructure(ContextId contextId, ITestLogger logger) : base(contextId, logger)
        {
        }

        /// <summary>Gets the Docker container managed by this infrastructure.</summary>
        protected TContainer Container { get; init; } = default!;

        /// <inheritdoc/>
        public override async Task Destroy()
        {
            await Container.StopAsync();
        }

        /// <inheritdoc/>
        public override async Task Initialize()
        {
            await Container.StartAsync();
        }

    }
}

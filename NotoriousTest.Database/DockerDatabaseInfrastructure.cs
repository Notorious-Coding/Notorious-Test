using DotNet.Testcontainers.Containers;

using NotoriousTest.Logger;

using System.Data.Common;

namespace NotoriousTest.Database
{
    /// <summary>
    /// Base class for database infrastructures backed by a Testcontainers Docker container.
    /// </summary>
    /// <typeparam name="TContainer">The Testcontainers database container type.</typeparam>
    /// <typeparam name="TOutputConfiguration">The type of configuration value produced by this infrastructure.</typeparam>
    public abstract class DockerDatabaseInfrastructure<TContainer, TOutputConfiguration> : DatabaseInfrastructureBase<TOutputConfiguration> where TContainer : IDatabaseContainer
    {
        /// <summary>Initializes a new instance with the given context and logger.</summary>
        /// <param name="contextId">The environment context identifier.</param>
        /// <param name="logger">The test logger.</param>
        protected DockerDatabaseInfrastructure(ContextId contextId, ITestLogger logger) : base(contextId, logger)
        {
        }

        /// <summary>Gets the Testcontainers database container managed by this infrastructure.</summary>
        protected TContainer Container { get; init; } = default!;

        /// <inheritdoc/>
        public override async Task Initialize()
        {
            await Container.StartAsync();
            await base.Initialize();
        }

        /// <inheritdoc/>
        public override async Task Destroy()
        {
            await Container.StopAsync();
        }

        /// <inheritdoc/>
        public override string GetServerConnectionString()
        {
            return Container.GetConnectionString();
        }

        /// <inheritdoc/>
        protected override async Task DropDatabase(DbConnection connection)
        {
            // Dropping the database is unnecessary since the Docker container is destroyed with it.
        }
    }
}

using DotNet.Testcontainers.Containers;

using System.Data.Common;

namespace NotoriousTest.Database
{
    public abstract class DockerDatabaseInfrastructure<TContainer, TOutputConfiguration> : DatabaseInfrastructureBase<TOutputConfiguration> where TContainer : IDatabaseContainer
    {
        protected TContainer Container { get; init; }

        public override async Task Initialize()
        {
            await Container.StartAsync();
            await base.Initialize();
        }

        public override async Task Destroy()
        {
            await Container.StopAsync();
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

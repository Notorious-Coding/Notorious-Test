using NotoriousTest.Infrastructures;

namespace NotoriousTest.Environments
{
    public class AsyncEnvironmentConfig
    {
        public List<AsyncInfrastructure> Infrastructures { get; private set; } = new List<AsyncInfrastructure>();

        public AsyncEnvironmentConfig AddInfrastructures(AsyncInfrastructure infrastructure)
        {
            Infrastructures.Add(infrastructure);
            return this;
        }
    }
}

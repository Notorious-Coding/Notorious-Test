using NotoriousTest.Infrastructures;

namespace NotoriousTest.Environments
{
    public class EnvironmentConfig
    {
        public List<Infrastructure> Infrastructures { get; private set; } = new List<Infrastructure>();

        public EnvironmentConfig AddInfrastructures(Infrastructure infrastructure)
        {
            Infrastructures.Add(infrastructure);
            return this;
        }
    }
}

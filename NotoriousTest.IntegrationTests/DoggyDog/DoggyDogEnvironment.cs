using NotoriousTest.Sqlite;

using System.Reflection;

using Xunit.Sdk;

namespace NotoriousTest.IntegrationTests.DoggyDog
{
    public class DoggyDogEnvironment : XUnit.Environment
    {
        public DoggyDogEnvironment(IMessageSink sink) : base(sink)
        {
        }

        public override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

        public override async Task ConfigureEnvironment()
        {
            AddInfrastructure<SqliteInfrastructure>();
        }
    }
}

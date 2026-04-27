using System.Reflection;
using NotoriousTest.IntegrationTests.Infrastructure;
using Xunit.Sdk;

namespace NotoriousTest.IntegrationTests.SystemUnderTest;

public class NotoriousTestEnvironment(IMessageSink sink) : XUnit.Environment(sink)
{
    public override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

    public override Task ConfigureEnvironment() => Task.FromResult(AddInfrastructure<NotoriousTestRegistryInfrastructure>());
}

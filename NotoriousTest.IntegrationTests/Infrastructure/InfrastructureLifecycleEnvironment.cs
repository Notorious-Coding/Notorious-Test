using Dapper;

using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Settings;
using NotoriousTest.Sqlite;

using System.Reflection;

using Xunit.Sdk;

namespace NotoriousTest.IntegrationTests.Infrastructure;

public class InfrastructureLifecycleEnvironment : XUnit.Environment
{
    public InfrastructureLifecycleEnvironment(IMessageSink sink) : base(sink) { }

    public override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

    public override async Task ConfigureEnvironment()
    {
        AddInfrastructure<RegistryDatabaseInfrastructure>();
    }
}

public class RegistryDatabaseInfrastructure : SqliteInfrastructure
{
    public RegistryDatabaseInfrastructure(EnvironmentId contextId, ITestSettingsProvider settingsProvider, ITestLogger logger, IRegistry registry)
        : base(contextId, settingsProvider, logger, registry) { }

    public override async Task Initialize()
    {
        await base.Initialize();
        using var connection = GetDatabaseConnection();
        await connection.ExecuteAsync(DoggyDogTestFramework.Arrange.ENSURE_REGISTRY);
    }
}

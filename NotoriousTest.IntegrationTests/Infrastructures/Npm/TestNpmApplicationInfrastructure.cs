using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Web.Npm;

namespace NotoriousTest.IntegrationTests.Npm;

public class TestNpmApplicationInfrastructure : NpmWebApplicationInfrastructure
{
    public TestNpmApplicationInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry) :
        base(contextId, logger, registry)
    {
        AdditionalConfiguration = new Dictionary<string, string> { ["NOTORIOUS_TEST_KEY"] = "NotoriousValue" };
    }

    // From bin/Debug/net10.0 up to the repository root.
    protected override string ProjectDir { get; } = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "../../../../NotoriousTest.IntegrationsTest.Npm"));

    protected override string Command => "start";

    protected override string HealthCheckPath => "/health";

    // The test application reads its configuration from its environment variables.
    protected override Task ConfigureApplication(Dictionary<string, string?> configuration,
        IDictionary<string, string?> environmentVariables)
    {
        foreach ((string key, string? value) in configuration) environmentVariables[key] = value;
        return Task.CompletedTask;
    }
}

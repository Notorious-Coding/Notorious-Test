using NotoriousTest.Core;
using NotoriousTest.Core.Configuration;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Web.Helpers;
using NotoriousTest.Web.Npm;

namespace NotoriousTest.IntegrationTests.Npm;

public class TestNpmApplicationInfrastructure : NpmWebApplicationInfrastructure
{
    // From bin/Debug/net10.0 up to the repository root.
    protected override string ProjectDir { get; } = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "../../../../NotoriousTest.IntegrationsTest.Npm"));

    protected override string Command => "start";

    protected override string HealthCheckPath => "/health";

    public TestNpmApplicationInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry) :
        base(contextId, logger, registry)
    {
        AdditionalConfiguration = new Dictionary<string, string> { ["NOTORIOUS_TEST_KEY"] = "NotoriousValue" };
    }

    // The test application reads its configuration from its environment variables.
    protected override Task<Dictionary<string, string>>
        ConfigureApplication(List<ConfigurationEntry<object>> configuration) =>
        Task.FromResult(configuration.ToAppSettings().ToDictionary(x => x.Key, x => x.Value ?? string.Empty));
}

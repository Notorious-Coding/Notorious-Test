using NotoriousTest.Dependencies.Azure.FunctionCoreTools;

namespace NotoriousTest.IntegrationTests.AzureFunction;

/// <summary>
///     Installs the func CLI for the tests, as the environment would do, and uninstalls it afterward
///     only if it was not present beforehand.
/// </summary>
public class AzureFunctionCoreToolsFixture : IAsyncLifetime
{
    private readonly AzureFunctionCoreToolsDependency dependency = new();
    private bool installedByFixture;

    public async ValueTask InitializeAsync()
    {
        if (await dependency.Exist()) return;

        await dependency.Install();
        installedByFixture = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (installedByFixture) await dependency.Uninstall();
    }
}

/// <summary>
///     Tests installing or uninstalling the func CLI must not run in parallel with each other.
/// </summary>
[CollectionDefinition(NAME, DisableParallelization = true)]
public class AzureFunctionCoreToolsCollection
{
    public const string NAME = "AzureFunctionCoreTools";
}

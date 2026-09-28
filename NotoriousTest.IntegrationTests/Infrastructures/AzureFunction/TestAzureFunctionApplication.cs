using NotoriousTest.Web.AzureFunctions;

namespace NotoriousTest.IntegrationTests.AzureFunction;

public class TestAzureFunctionApplication : AzureFunctionWebApplication
{
    // From bin/Debug/net10.0 up to the repository root.
    public override string FunctionProjectDir { get; } = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "../../../../NotoriousTest.IntegrationsTest.Functions"));
}

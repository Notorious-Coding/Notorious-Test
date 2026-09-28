using NotoriousTest.Core.Infrastructures.Dependencies;
using NotoriousTest.Dependencies.Azure.FunctionCoreTools;
using NotoriousTest.IntegrationTests.AzureFunction;

namespace NotoriousTest.IntegrationTests.Dependencies;

/// <summary>
///     These tests share the machine state (func CLI installed or not) and must run in order.
/// </summary>
[Collection(AzureFunctionCoreToolsCollection.NAME)]
public class AzureFunctionCoreToolsDependencyTests : DependencyTestBase
{
    protected override IInfrastructureDependency Dependency => new AzureFunctionCoreToolsDependency();

    protected override Func<Task<ProcessResult>> DependencyProcessRunner =>
        () => ProcessRunner.RunAsync("func", "--version");
}

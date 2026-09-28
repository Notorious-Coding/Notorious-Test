using System.ComponentModel;
using NotoriousTest.Core.Infrastructures.Dependencies;
using NotoriousTest.Dependencies.Azure.FunctionCoreTools;
using NotoriousTest.IntegrationTests.Ordering;

namespace NotoriousTest.IntegrationTests.Dependencies;

[TestMethodOrderer(typeof(OrderAttributeTestMethodOrderer))]
public abstract class DependencyTestBase
{
    protected abstract IInfrastructureDependency Dependency { get; }

    protected abstract Func<Task<ProcessResult>> DependencyProcessRunner { get; }

    [Fact]
    [Order(1)]
    public async Task Dependency_Exists_Should_Return_False_If_Not_Installed()
    {
        bool exist = await Dependency.Exist();

        Assert.False(exist);
    }

    [Fact]
    [Order(2)]
    public async Task Dependency_Should_Install_AzureFunctionCoreTools_Correctly()
    {
        await Dependency.Install();

        ProcessResult result = await DependencyProcessRunner();
        Assert.True(result.Succeeded);
    }

    [Fact]
    [Order(3)]
    public async Task Dependency_Exists_Should_Return_True_If_Installed()
    {
        bool exist = await Dependency.Exist();

        Assert.True(exist);
    }

    [Fact]
    [Order(4)]
    public async Task Dependency_Should_Uninstall_If_Installed()
    {
        await Dependency.Uninstall();
        await Assert.ThrowsAsync<Win32Exception>(() => DependencyProcessRunner());
    }
}

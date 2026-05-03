using NotoriousTest.Core;
using NotoriousTest.Core.Environments;
using Xunit;
using Xunit.v3;

namespace NotoriousTest.XUnit;

public class XUnitFixture<TEnvironment> : Fixture<TEnvironment>, IAsyncLifetime where TEnvironment : EnvironmentBase
{
    public XUnitFixture() : base((TestContext.Current.TestClass as XunitTestClass)?.Class)
    {
    }

    public async ValueTask InitializeAsync() => await Environment.Initialize();
    public async ValueTask DisposeAsync() => await Environment.Destroy();
}

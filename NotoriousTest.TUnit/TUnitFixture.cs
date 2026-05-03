using NotoriousTest.Core;
using NotoriousTest.Core.Environments;

using TUnit.Core.Interfaces;

namespace NotoriousTest.TUnit;

public class TUnitFixture<TEnvironment> : Fixture<TEnvironment>, IAsyncInitializer, IAsyncDisposable where TEnvironment : EnvironmentBase
{

    public TUnitFixture(): base(TestBuilderContext.Current?.TestMetadata.Class.Type)
    {

    }

    public async Task InitializeAsync() => await Environment.Initialize();
    public async ValueTask DisposeAsync() => await Environment.Destroy();
}

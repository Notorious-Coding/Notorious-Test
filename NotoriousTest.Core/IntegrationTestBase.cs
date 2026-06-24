using NotoriousTest.Core.DI;
using NotoriousTest.Core.Environments;

namespace NotoriousTest.Core;

[InjectionConfigurator(typeof(DependencyInjectionConfigurator))]
public abstract class IntegrationTestBase<T> where T : EnvironmentBase
{
    protected T Environment => Fixture.Environment;

    [Obsolete("Use Environment instead")] protected T CurrentEnvironment => Fixture.Environment;

    protected Fixture<T> Fixture { get; set; }
}

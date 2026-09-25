using NotoriousTest.Core.DI;
using NotoriousTest.Core.Environments;

namespace NotoriousTest.Core;

/// <summary>
///     Base class for integration test class.
/// </summary>
/// <typeparam name="T">Environment to set up.</typeparam>
[InjectionConfigurator(typeof(DependencyInjectionConfigurator))]
public abstract class IntegrationTestBase<T> where T : EnvironmentBase
{
    /// <summary>
    ///     Current environment.
    /// </summary>
    protected T Environment => Fixture.Environment;

    [Obsolete("Use Environment instead")] protected T CurrentEnvironment => Fixture.Environment;

    /// <summary>
    ///     Fixture used to instantiate the environment.
    /// </summary>
    protected Fixture<T> Fixture { get; set; }
}

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NotoriousTest.Core.DI;
using NotoriousTest.Core.Environments;

namespace NotoriousTest.Core;

/// <summary>
///     Class responsible for instantiation an environment.
/// </summary>
/// <typeparam name="TEnvironment">Environment to instantiate.</typeparam>
public class Fixture<TEnvironment> where TEnvironment : EnvironmentBase
{
    /// <summary>
    ///     Creates a new instance of <see cref="Fixture{TEnvironment}" />.
    /// </summary>
    /// <param name="testClass">Current executing test class.</param>
    public Fixture(Type? testClass)
    {
        TestClass = testClass;
        IServiceProvider services = ConfigureServiceCollection();
        Environment = InstantiateEnvironment(services);
    }

    /// <summary>
    ///     Instantiated environment.
    /// </summary>
    public TEnvironment Environment { get; private set; }

    /// <summary>
    ///     Test class instantiating the environment.
    /// </summary>
    protected Type? TestClass { get; }

    /// <summary>
    ///     List of all dependency injection configurators applied to the test class.
    /// </summary>
    protected IDependencyInjectionConfigurator[] InjectionConfigurators => field ??= GetInjectionConfigurators();

    private TEnvironment InstantiateEnvironment(IServiceProvider provider) =>
        ActivatorUtilities.CreateInstance<TEnvironment>(provider);

    private IServiceProvider ConfigureServiceCollection()
    {
        IServiceCollection services = new ServiceCollection();

        foreach (IDependencyInjectionConfigurator dependencyInjectionConfigurator in InjectionConfigurators)
            dependencyInjectionConfigurator.ConfigureServices(services);

        return services.BuildServiceProvider();
    }

    private IDependencyInjectionConfigurator[] GetInjectionConfigurators()
    {
        IEnumerable<Type> diConfiguratorTypes = TestClass!.GetCustomAttributes<InjectionConfiguratorAttribute>()
            .Select(ic => ic.DIConfiguratorType).ToArray();
        return diConfiguratorTypes.Select(dic => Activator.CreateInstance(dic) as IDependencyInjectionConfigurator)
            .Where(dic => dic is not null).ToArray()!;
    }
}

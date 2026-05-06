using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NotoriousTest.Core.Environments;
using NotoriousTest.Core.DI;

namespace NotoriousTest.Core;

public class Fixture<TEnvironment> where TEnvironment : EnvironmentBase
{
    public TEnvironment Environment { get; private set; }
    protected Type? TestClass { get;  }
    protected IDependencyInjectionConfigurator[] InjectionConfigurators => field ??=  GetInjectionConfigurators();
    private readonly IServiceProvider _services;

    public Fixture(Type? testClass)
    {
        TestClass = testClass;
        _services = ConfigureServiceCollection();
        Environment = InstantiateEnvironment(_services);
    }

    private TEnvironment InstantiateEnvironment(IServiceProvider provider) => ActivatorUtilities.CreateInstance<TEnvironment>(provider);

    private IServiceProvider ConfigureServiceCollection()
    {
        IServiceCollection services = new ServiceCollection();

        foreach (IDependencyInjectionConfigurator dependencyInjectionConfigurator in InjectionConfigurators)
        {
            dependencyInjectionConfigurator.ConfigureServices(services);
        }

        return services.BuildServiceProvider();
    }

    private IDependencyInjectionConfigurator[] GetInjectionConfigurators()
    {
        IEnumerable<Type> diConfiguratorTypes = TestClass!.GetCustomAttributes<InjectionConfiguratorAttribute>()
            .Select(ic => ic.DIConfiguratorType).ToArray();
        return diConfiguratorTypes.Select((dic) => Activator.CreateInstance(dic) as IDependencyInjectionConfigurator).Where(dic => dic is not null).ToArray()!;
    }
}

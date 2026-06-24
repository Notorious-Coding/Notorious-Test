using NotoriousTest.Core.DI;
using NotoriousTest.Core.Environments;
using DependencyInjectionConfigurator = NotoriousTest.DI.DependencyInjectionConfigurator;

namespace NotoriousTest;

[InjectionConfigurator(typeof(DependencyInjectionConfigurator))]
public abstract class IntegrationTestBase<T> : Core.IntegrationTestBase<T> where T : EnvironmentBase;

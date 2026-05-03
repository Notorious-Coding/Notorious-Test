using NotoriousTest.Core.DI;

namespace NotoriousTest.UnitTests;

[InjectionConfigurator(typeof(DIConfiguratorStub))]
[InjectionConfigurator(typeof(DIConfiguratorStub2))]
public class IntegrationTestStub{

}

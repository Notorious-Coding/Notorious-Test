using AwesomeAssertions;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using NotoriousTest.Core;
using NotoriousTest.Core.DI;
using NotoriousTest.Core.Environments;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Runtime;
using NotoriousTest.Core.Watchdog;
using NotoriousTest.UnitTests.Stubs;

namespace NotoriousTest.UnitTests;



public class FixtureTests
{
    [Fact]
    public Task Fixture_Should_ConfigureEnvironmentWithInjectionAttributes()
    {
        var fixture = new Fixture<EnvironmentStub>(typeof(IntegrationTestStub));
        fixture.Environment.PublicServiceProvider.GetService<IWatchDog>().Should().NotBeNull();
        fixture.Environment.PublicServiceProvider.GetService<ITestLogger>().Should().NotBeNull();
        fixture.Environment.PublicServiceProvider.GetService<IRegistry>().Should().NotBeNull();
        fixture.Environment.PublicServiceProvider.GetService<IRuntime>().Should().NotBeNull();
        fixture.Environment.PublicServiceProvider.GetService<EnvironmentSettings>().Should().NotBeNull();
        return Task.CompletedTask;
    }
}

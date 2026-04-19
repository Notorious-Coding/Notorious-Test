using AwesomeAssertions;

using NotoriousTest.Core;
using NotoriousTest.Core.Infrastructures.Cleaner;

using System.Reflection;

namespace NotoriousTest.UnitTests.Core;

public class CleanerAttributeTests
{

    class InfrastructureCleaner : IInfrastructureCleaner
    {
        public Task CleanAfterCrash(EnvironmentId contextId, Guid infrastructureId, object? metadata = null)
        {
            return Task.CompletedTask;
        }
    }

    [Cleaner(typeof(InfrastructureCleaner))]
    class CleanedInfrastructure
    {

    }

    class InheritedCleanedInfrastructure : CleanedInfrastructure
    {

    }



    [Fact]
    public void CleanerAttribute_Should_StoreCleanerType()
        => new CleanerAttribute(typeof(InfrastructureCleaner)).CleanerType.Should().Be(typeof(InfrastructureCleaner));

    [Fact]
    public void CleanerAttribute_Should_BeFoundOnSubclassViaGetCustomAttribute()
        => new InheritedCleanedInfrastructure().GetType().GetCustomAttribute<CleanerAttribute>().Should().NotBeNull();
}

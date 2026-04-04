using AwesomeAssertions;

using NotoriousTest.Core;
using NotoriousTest.Core.Infrastructures.Cleaner;

using System.Reflection;

namespace NotoriousTest.UnitTests.Core;

public class CleanerAttributeTests
{

    class InfrastructureCleaner : IInfrastructureCleaner
    {
        public Task CleanAfterCrash(ContextId contextId, Guid infrastructureId, object? metadata = null)
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
    public void CleanerAttribute_StoresCleanerType()
        => new CleanerAttribute(typeof(InfrastructureCleaner)).CleanerType.Should().Be(typeof(InfrastructureCleaner));

    [Fact]
    public void CleanerAttribute_Inherited_FoundOnSubclassViaGetCustomAttribute()
        => new InheritedCleanedInfrastructure().GetType().GetCustomAttribute<CleanerAttribute>().Should().NotBeNull();
}

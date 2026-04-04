using AwesomeAssertions;

using NotoriousTest.Core;

namespace NotoriousTest.UnitTests.Core;

public class ContextIdTests
{
    [Fact]
    public void ContextId_ImplicitConversionFromGuid_PreservesValue()
    {
        Guid expectedGuid = Guid.NewGuid();
        EnvironmentId guid = expectedGuid;

        guid.Value.Should().Be(expectedGuid);
    }

    [Fact]
    public void ContextId_ImplicitConversionToGuid_PreservesValue()
    {
        EnvironmentId expectedGuid = new EnvironmentId(Guid.NewGuid());
        Guid guid = expectedGuid;

        guid.Should().Be(expectedGuid.Value);
    }
}

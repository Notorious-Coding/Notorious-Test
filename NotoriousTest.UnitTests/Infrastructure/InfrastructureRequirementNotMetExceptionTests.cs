using AwesomeAssertions;

using NotoriousTest.Core.Infrastructures.Dependencies;

namespace NotoriousTest.UnitTests.Infrastructure;

public class InfrastructureRequirementNotMetExceptionTests
{
    private class FirstRequirement : IInfrastructureRequirement
    {
        public Task<bool> Exist() => Task.FromResult(false);
    }

    private class SecondRequirement : IInfrastructureRequirement
    {
        public Task<bool> Exist() => Task.FromResult(false);
    }

    [Fact]
    public void Message_Should_ContainNotMetRequirementNames()
    {
        InfrastructureRequirementNotMetException exception = new([new FirstRequirement(), new SecondRequirement()]);

        exception.Message.Should().Contain(nameof(FirstRequirement)).And.Contain(nameof(SecondRequirement));
    }

    [Fact]
    public void Requirements_Should_ExposeNotMetRequirements()
    {
        FirstRequirement first = new();
        SecondRequirement second = new();

        InfrastructureRequirementNotMetException exception = new([first, second]);

        exception.Requirements.Should().Equal(first, second);
    }
}

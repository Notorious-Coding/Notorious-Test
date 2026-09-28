namespace NotoriousTest.Core.Infrastructures.Dependencies;

/// <summary>
///     Thrown when one or more <see cref="IInfrastructureRequirement" /> are not met and cannot be installed.
/// </summary>
public class InfrastructureRequirementNotMetException : Exception
{
    public InfrastructureRequirementNotMetException(IEnumerable<IInfrastructureRequirement> requirements)
        : base("The following requirements are not met on this machine: " +
               string.Join(", ", requirements.Select(r => r.GetType().Name)) +
               ". Install them before running the tests.")
    {
        Requirements = requirements.ToList();
    }

    /// <summary>
    ///     Requirements that are not met.
    /// </summary>
    public IReadOnlyList<IInfrastructureRequirement> Requirements { get; }
}

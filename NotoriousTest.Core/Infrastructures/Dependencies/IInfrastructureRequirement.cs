namespace NotoriousTest.Core.Infrastructures.Dependencies;

/// <summary>
///     Represents a piece of software/hardware that must be present for the infrastructure to work,
///     but that NotoriousTest will not install by itself (e.g., Docker).
///     If the requirement is not met, the environment initialization fails before any infrastructure starts.
/// </summary>
public interface IInfrastructureRequirement
{
    /// <summary>
    ///     Verifies if the requirement is met on the software running the infrastructure.
    /// </summary>
    Task<bool> Exist();
}

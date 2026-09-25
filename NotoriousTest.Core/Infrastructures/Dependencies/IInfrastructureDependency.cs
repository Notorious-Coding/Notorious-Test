namespace NotoriousTest.Core.Infrastructures.Dependencies;

/// <summary>
///     Represents a piece of software/hardware needed for the infrastructure to work.
///     E.g., Docker, Node, etc.
/// </summary>
public interface IInfrastructureDependency
{
    /// <summary>
    ///     Installs the dependency on the software running the infrastructure.
    /// </summary>
    Task Install();

    /// <summary>
    ///     Uninstall the dependency on the software running the infrastructure.
    /// </summary>
    Task Uninstall();

    /// <summary>
    ///     Verifies if the dependency is already installed.
    /// </summary>
    Task<bool> Exist();
}

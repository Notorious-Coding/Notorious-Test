using NotoriousTest.Core.Infrastructures.Dependencies;

namespace NotoriousTest.Core.Infrastructures;

/// <summary>
///     Interface for running infrastructure.
/// </summary>
public interface IInfrastructure
{
    /// <summary>
    ///     If true, the infrastructure will be reset after each test.
    /// </summary>
    bool AutoReset { get; set; }

    /// <summary>
    ///     Environment identifier where the infrastructure was created.
    /// </summary>
    EnvironmentId EnvironmentId { get; set; }

    /// <summary>
    ///     Execution order of the infrastructre.
    /// </summary>
    int? Order { get; }

    /// <summary>
    ///     List of dependencies that needs installation before initialization.
    /// </summary>
    List<IInfrastructureDependency> Dependencies { get; set; }

    /// <summary>
    ///     List of requirements that needs to be met before initialization.
    /// </summary>
    List<IInfrastructureRequirement> Requirements { get; set; }

    /// <summary>
    ///     Clean the infrastructure from the software it has been initialized.
    /// </summary>
    Task Destroy();

    /// <summary>
    ///     Start the infrastructure on the running software.
    /// </summary>
    Task Initialize();

    /// <summary>
    ///     Reset the infrastructure to a clean state.
    /// </summary>
    Task Reset();
}

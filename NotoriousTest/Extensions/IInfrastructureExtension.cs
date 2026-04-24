using NotoriousTest.Infrastructures;

/// <summary>
/// Allows extending infrastructure lifecycle with custom hooks for initialize, reset, and destroy events.
/// </summary>
public interface IInfrastructureExtension
{
    /// <summary>
    /// Called before the infrastructure is initialized.
    /// </summary>
    Task OnBeforeInitialize(IInfrastructure infrastructure) => Task.CompletedTask;

    /// <summary>
    /// Called after the infrastructure is initialized.
    /// </summary>
    Task OnAfterInitialize(IInfrastructure infrastructure) => Task.CompletedTask;

    /// <summary>
    /// Called before the infrastructure is reset.
    /// </summary>
    Task OnBeforeReset(IInfrastructure infrastructure) => Task.CompletedTask;

    /// <summary>
    /// Called after the infrastructure is reset.
    /// </summary>
    Task OnAfterReset(IInfrastructure infrastructure) => Task.CompletedTask;

    /// <summary>
    /// Called before the infrastructure is destroyed.
    /// </summary>
    Task OnBeforeDestroy(IInfrastructure infrastructure) => Task.CompletedTask;

    /// <summary>
    /// Called after the infrastructure is destroyed.
    /// </summary>
    Task OnAfterDestroy(IInfrastructure infrastructure) => Task.CompletedTask;
}

/// <summary>
/// Strongly-typed variant of <see cref="IInfrastructureExtension"/> that receives a specific infrastructure type.
/// </summary>
/// <typeparam name="T">The concrete infrastructure type this extension handles.</typeparam>
public interface IInfrastructureExtension<T> : IInfrastructureExtension where T : IInfrastructure
{
    Task IInfrastructureExtension.OnBeforeInitialize(IInfrastructure infrastructure)
        => infrastructure is T typed ? OnBeforeInitialize(typed) : Task.CompletedTask;

    Task IInfrastructureExtension.OnAfterInitialize(IInfrastructure infrastructure)
        => infrastructure is T typed ? OnAfterInitialize(typed) : Task.CompletedTask;

    Task IInfrastructureExtension.OnBeforeReset(IInfrastructure infrastructure)
        => infrastructure is T typed ? OnBeforeReset(typed) : Task.CompletedTask;

    Task IInfrastructureExtension.OnAfterReset(IInfrastructure infrastructure)
        => infrastructure is T typed ? OnAfterReset(typed) : Task.CompletedTask;

    Task IInfrastructureExtension.OnBeforeDestroy(IInfrastructure infrastructure)
        => infrastructure is T typed ? OnBeforeDestroy(typed) : Task.CompletedTask;

    Task IInfrastructureExtension.OnAfterDestroy(IInfrastructure infrastructure)
        => infrastructure is T typed ? OnAfterDestroy(typed) : Task.CompletedTask;

    /// <summary>
    /// Called before the infrastructure is initialized.
    /// </summary>
    Task OnBeforeInitialize(T infrastructure) => Task.CompletedTask;

    /// <summary>
    /// Called after the infrastructure is initialized.
    /// </summary>
    Task OnAfterInitialize(T infrastructure) => Task.CompletedTask;

    /// <summary>
    /// Called before the infrastructure is reset.
    /// </summary>
    Task OnBeforeReset(T infrastructure) => Task.CompletedTask;

    /// <summary>
    /// Called after the infrastructure is reset.
    /// </summary>
    Task OnAfterReset(T infrastructure) => Task.CompletedTask;

    /// <summary>
    /// Called before the infrastructure is destroyed.
    /// </summary>
    Task OnBeforeDestroy(T infrastructure) => Task.CompletedTask;

    /// <summary>
    /// Called after the infrastructure is destroyed.
    /// </summary>
    Task OnAfterDestroy(T infrastructure) => Task.CompletedTask;
}

using NotoriousTest.Infrastructures;

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
    new Task OnBeforeInitialize(T infrastructure) => Task.CompletedTask;

    /// <summary>
    /// Called after the infrastructure is initialized.
    /// </summary>
    new Task OnAfterInitialize(T infrastructure) => Task.CompletedTask;

    /// <summary>
    /// Called before the infrastructure is reset.
    /// </summary>
    new Task OnBeforeReset(T infrastructure) => Task.CompletedTask;

    /// <summary>
    /// Called after the infrastructure is reset.
    /// </summary>
    new Task OnAfterReset(T infrastructure) => Task.CompletedTask;

    /// <summary>
    /// Called before the infrastructure is destroyed.
    /// </summary>
    new Task OnBeforeDestroy(T infrastructure) => Task.CompletedTask;

    /// <summary>
    /// Called after the infrastructure is destroyed.
    /// </summary>
    new Task OnAfterDestroy(T infrastructure) => Task.CompletedTask;
}
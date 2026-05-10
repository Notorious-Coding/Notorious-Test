namespace DoggyDog.UI.Context;

public abstract class Context : IContext
{
    private readonly SynchronizationContext _context = SynchronizationContext.Current!;
    public virtual Task OnMount()
    {
        IsMounted = true;
        return Task.CompletedTask;
    }
    protected bool IsMounted { get; set; }

    public void ExecuteOnUi(Action<object?> action) => _context.Post(state => action(state), null);
}

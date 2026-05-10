using System.ComponentModel;
using Terminal.Gui.ViewBase;

namespace DoggyDog.UI;

public abstract class AsyncView : View
{
    protected AsyncView()
    {
        Initialized += async (_, __) => await InitAsync();
    }

    protected virtual Task InitAsync() => Task.CompletedTask;
}

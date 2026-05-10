using System.Collections.ObjectModel;
using DoggyDog.UI.Tree.Environments;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace DoggyDog.UI.Tree;

public class Main : Runnable
{
    public Main()
    {
        var environmentDashboard = new EnvironmentDashboard {Height = Dim.Fill(), Width = Dim.Fill(), };
        Add(environmentDashboard);

        Initialized += async (s, e) => await InitAsync();
    }

    public Task InitAsync() => Task.CompletedTask;
}

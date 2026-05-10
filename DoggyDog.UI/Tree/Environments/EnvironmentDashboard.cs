using System.Reflection.Emit;
using Terminal.Gui.ViewBase;

namespace DoggyDog.UI.Tree.Environments;

public class EnvironmentDashboard : View
{
    private EnvironmentList _list;
    private EnvironmentDetails _details;

    public EnvironmentDashboard()
    {
        _details = new EnvironmentDetails
        {
            Width = Dim.Percent(70),
            Height = Dim.Fill(),
        };

        _list = new EnvironmentList
        {
            Width =  Dim.Fill(),
            Height = Dim.Fill(),
            X = Pos.Right(_details),
        };

        _list.ValueChanged += (sender, args) =>
        {
            _details.CurrentEnvironment = args.NewValue;
        };

        Add(_list);
        Add(_details);
    }
}

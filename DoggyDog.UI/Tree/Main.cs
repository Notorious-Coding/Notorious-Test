using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace DoggyDog.UI.Tree;

public class Main : Runnable
{
    public Main()
    {
        CanFocus = true;
        Border.Thickness = new Thickness(1);
        BorderStyle = LineStyle.Rounded;

        var header = new Header { Height = Dim.Auto(), Width = Dim.Fill() };
        header.Margin.Thickness = new Thickness(1, 0, 1, 0);

        var body = new Body.Body { Height = Dim.Fill(), Width = Dim.Fill(), Y = Pos.Bottom(header) };

        Add(header);
        Add(body);
    }
}

using DoggyDog.UI.Tree;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;

using IApplication app = Application.Create ();

app.Init ();

var main = new Main()
{
    Width = Dim.Fill(),
    Height = Dim.Fill(),
};
app.Run (main);

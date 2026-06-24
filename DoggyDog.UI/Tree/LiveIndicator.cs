using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Attribute = Terminal.Gui.Drawing.Attribute;

namespace DoggyDog.UI.Tree;

public class LiveIndicator : View
{
    private readonly Label _dot;
    private bool _pulse;
    private object? _timeOutToken;

    public LiveIndicator()
    {
        Label live = new() { Text = "LIVE", Height = 1, Width = 4, X = 0 };
        _dot = new Label { Text = "●", X = Pos.Right(live) + 1, Width = 1, Height = 1 };
        _dot.SetScheme(MakeDotScheme(false));

        Add(_dot);
        Add(live);
    }

    public override void EndInit()
    {
        base.EndInit();
        _timeOutToken = App?.AddTimeout(TimeSpan.FromMilliseconds(500), () =>
        {
            Tick();
            return true;
        });
    }

    private void Tick()
    {
        _pulse = !_pulse;
        _dot.SetScheme(MakeDotScheme(_pulse));
    }

    private static Scheme MakeDotScheme(bool bright) => new()
    {
        Normal = new Attribute(bright ? ColorName16.BrightRed : ColorName16.Red, ColorName16.Black)
    };

    protected override void Dispose(bool disposing)
    {
        if (disposing && _timeOutToken != null) App?.RemoveTimeout(_timeOutToken);

        base.Dispose(disposing);
    }
}

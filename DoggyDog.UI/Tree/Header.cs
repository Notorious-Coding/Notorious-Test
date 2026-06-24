using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace DoggyDog.UI.Tree;

public class Header : View
{
    private readonly LiveIndicator _liveIndicator;
    private readonly Label _timeLabel;
    private object? _timeToken;

    public Header()
    {
        var label = new Label { Text = "DoggyDog ▶ Infrastructure Monitor" };


        _liveIndicator = new LiveIndicator
        {
            X = Pos.Align(Alignment.End, AlignmentModes.StartToEnd), Height = Dim.Auto(), Width = Dim.Auto()
        };
        _timeLabel = new Label
        {
            Text = DateTime.Now.ToString("HH:mm:ss"), X = Pos.Align(Alignment.End, AlignmentModes.StartToEnd)
        };
        _timeLabel.Margin.Thickness = new Thickness(0, 0, 1, 0);

        var separator = new Line { Y = Pos.Bottom(label) };

        Add(label);
        Add(_timeLabel);
        Add(_liveIndicator);
        Add(separator);
    }

    public override void EndInit()
    {
        base.EndInit();
        _timeToken = App?.AddTimeout(TimeSpan.FromSeconds(1), UpdateTime);
    }

    private bool UpdateTime()
    {
        _timeLabel.Text = DateTime.Now.ToString("HH:mm:ss");
        return true;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _timeToken != null) App?.RemoveTimeout(_timeToken);
        base.Dispose(disposing);
    }
}

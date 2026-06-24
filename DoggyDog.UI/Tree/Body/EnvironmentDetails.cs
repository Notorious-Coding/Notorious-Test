using System.Text;
using DoggyDog.UI.Context.Model;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Environment = DoggyDog.UI.Context.Model.Environment;

namespace DoggyDog.UI.Tree.Body;

public class EnvironmentDetails : View
{
    private readonly Environment _environment;
    private readonly Process _parent;

    public EnvironmentDetails(Environment environment, Process parent)
    {
        _environment = environment;
        _parent = parent;
        BorderStyle = LineStyle.Rounded;
        Border.Thickness = new Thickness(1);
        Padding.Thickness = new Thickness(1);
        Title = $"Environment {_environment.Id.ToString()[..8]}";
        VerticalScrollBar.Visible = true;
        VerticalScrollBar.VisibilityMode = ScrollBarVisibilityMode.Always;
        VerticalScrollBar.ViewportSettings = ViewportSettingsFlags.HasVerticalScrollBar;
        var processPidLabel = new Label { Text = $"PID - {_parent.Id}" };
        var testAssemblyLabel = new Label { Text = $"Test Assembly {parent.Name}", Y = Pos.Bottom(processPidLabel) };

        Add(testAssemblyLabel);
        Add(processPidLabel);

        IEnumerable<KeyValuePair<InfrastructureType, int>> envByType =
            environment.Infrastructures.CountBy(i => i.Type);

        var contentResume = new StringBuilder();

        foreach (KeyValuePair<InfrastructureType, int> infrastructure in envByType)
            contentResume.Append($"{infrastructure.Value} {infrastructure.Key} ·");

        var contentResumeLabel = new Label
        {
            Text = contentResume.ToString().Remove(contentResume.Length - 1), Y = Pos.Bottom(testAssemblyLabel)
        };
        Add(contentResumeLabel);

        View precedent = contentResumeLabel;
        foreach (Infrastructure infrastructure in environment.Infrastructures)
        {
            var infraDetails = new View
            {
                Title = $"{infrastructure.Type.ToString()} {infrastructure.Id.ToString()[..8]}",
                Width = Dim.Fill(),
                Height = Dim.Auto(),
                Y = Pos.Bottom(precedent) + 1,

            };

            infraDetails.BorderStyle = LineStyle.Rounded;
            infraDetails.Border.Thickness = new Thickness(1);
            infraDetails.Padding.Thickness = new Thickness(1);

            precedent = infraDetails;
            Add(infraDetails);
        }
    }
}

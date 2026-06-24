using Terminal.Gui.ViewBase;
using Environment = DoggyDog.UI.Context.Model.Environment;

namespace DoggyDog.UI.Tree.Body;

public class Body : View
{
    private Environment? _selectedEnvironment;

    public Body()
    {
        CanFocus = true;
        var processes = new ProcessTree
        {
            X = Pos.Align(Alignment.End),
            Width = Dim.Auto(DimAutoStyle.Content, Dim.Percent(30)),
            Height = Dim.Fill()
        };


        processes.SelectionChanged += (_, selected) =>
        {
            if (selected.NewValue?.Tag == null || selected.NewValue?.Tag is not ProcessTree.TreeMetadata) return;

            var metadata = (ProcessTree.TreeMetadata)selected.NewValue!.Tag;
            var details =
                new EnvironmentDetails(metadata.Environment, metadata.ParentProcess)
                {
                    Width = Dim.Fill(processes), Height = Dim.Fill()
                };

            Add(details);
            SetNeedsDraw();
        };


        Add(processes);
    }
}

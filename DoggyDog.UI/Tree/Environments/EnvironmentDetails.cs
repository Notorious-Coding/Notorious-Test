using DoggyDog.UI.Context.Model;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Environment = DoggyDog.UI.Context.Model.Environment;

namespace DoggyDog.UI.Tree;

public class EnvironmentDetails : FrameView
{
    public Environment? CurrentEnvironment
    {
        get;
        set
        {
            field = value;
            Update();
        }
    }


    public EnvironmentDetails()
    {
        Initialized += async (_, _) =>  await InitAsync();
    }

    private async Task InitAsync() => Title = "No environment selected";

    private void Update()
    {
        if (CurrentEnvironment == null)
            return;

        Title = CurrentEnvironment?.ToString() ?? "No environment selected";
        foreach (Infrastructure infrastructure in CurrentEnvironment?.Infrastructures)
        {
            var window = new Window();
            window.Title = $"{infrastructure.Name} - {infrastructure.Id.ToString()[..8]}";
            window.Width = 50;
            window.Height = 10;

            var infraTypeLabel = new Label();
            infraTypeLabel.Text = "Type: " +infrastructure.Type;
            infraTypeLabel.Height = Dim.Auto();
            infraTypeLabel.Width = Dim.Auto();
            window.Add(infraTypeLabel);

            var creationDateLabel = new Label();
            creationDateLabel.Text = "Created: " + infrastructure.CreationDate.ToLongTimeString();
            window.Add(creationDateLabel);

            var resetDateLabel = new Label();
            resetDateLabel.Text = "Resetted: " + (infrastructure.ResetDate?.ToLongTimeString() ?? "Never");
            resetDateLabel.Y = Pos.Bottom(creationDateLabel);
            window.Add(resetDateLabel);

            Add(window);
        }

        SetNeedsDraw();
    }
}

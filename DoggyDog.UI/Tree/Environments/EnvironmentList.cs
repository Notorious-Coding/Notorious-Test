using System.Collections.ObjectModel;
using DoggyDog.UI.Context;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Environment = DoggyDog.UI.Context.Model.Environment;

namespace DoggyDog.UI.Tree.Environments;

public class EnvironmentList : ListView<Environment>
{
    private Label _lastRefreshlabel = new(){ Text = " - ", Width = Dim.Auto(), Height = Dim.Auto() };
    private ListView<Environment> _list = new();
    public EnvironmentList()
    {
        Title = $"Environments";
        BorderStyle = LineStyle.Single;
        Border.Thickness = new Thickness(1);
        _list = new ListView<Environment>();

        Initialized += async (_, _) => await InitAsync();
    }

    private async Task InitAsync()
    {
        EnvironmentContext context = await DependencyInjection.Use<EnvironmentContext>();

        context.OnEnvironmentListRefresh += (hasChanged) =>
        {
            _lastRefreshlabel.Text = context.LastRefresh.ToLongTimeString();
            if (hasChanged)
            {
                Environment? selected = SelectedItem;
                SetSource(context.Environments);
                Title = $"{context.Environments.Count} Environments";

                if (selected is not null)
                {
                    int newIndex = context.Environments.IndexOf(selected);
                    if (newIndex >= 0) SetSelection(newIndex, false);
                }

                SetNeedsDraw();
            }
        };
        SetSource(context.Environments);
    }
}

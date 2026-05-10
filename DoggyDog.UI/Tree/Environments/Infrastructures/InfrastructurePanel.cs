using DoggyDog.UI.Context.Model;
using Terminal.Gui.ViewBase;

namespace DoggyDog.UI.Tree.Environments.Infrastructures;

public class InfrastructurePanel : View
{
    public Infrastructure? Infrastructure
    {
        get;
        set
        {
            field = value;
            Update();
        }
    }

    private void Update()
    {

        SetNeedsDraw();
    }
}

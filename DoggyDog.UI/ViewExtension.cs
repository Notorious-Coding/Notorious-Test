using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace DoggyDog.UI;

public static class ViewExtension
{
    extension(View view)
    {
        public FrameView WithFrame(string title)
        {
            var frameView = new FrameView { Title = title };
            frameView.Add(view);

            return frameView;
        }
    }

}

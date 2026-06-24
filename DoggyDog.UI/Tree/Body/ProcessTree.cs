using System.Text;
using DoggyDog.UI.Context;
using DoggyDog.UI.Context.Model;
using Terminal.Gui.Drawing;
using Terminal.Gui.Views;
using Environment = DoggyDog.UI.Context.Model.Environment;

namespace DoggyDog.UI.Tree.Body;

public class ProcessTree : TreeView
{
    public ProcessTree()
    {
        Border.Thickness = new Thickness(1);
        BorderStyle = LineStyle.Rounded;
        Title = "Processes";
        Style.CollapseableSymbol = new Rune('▶');
        Style.ExpandableSymbol = new Rune('▼');

        Initialized += async (_, __) =>
        {
            ProcessContext context = await DependencyInjection.Use<ProcessContext>();

            context.OnProcessListRefresh += _ => { App?.Invoke(SetNeedsDraw); };

            App?.Invoke(() =>
            {
                var root = new TreeNode { Text = "All" };

                if (!context.Processes.Any())
                {
                    root.Text = "No processes found";
                    return;
                }

                foreach (Process process in context.Processes)
                {
                    var processNode = new TreeNode { Text = process.Name, Tag = "process" };

                    var envNode = new TreeNode
                    {
                        Text = "Env :" + process.Environment.Id.ToString()[..8],
                        Tag = new TreeMetadata(process.Environment, process)
                    };
                    processNode.Children.Add(envNode);

                    root.Children.Add(processNode);
                }

                AddObject(root);
                ExpandAll();
                SetNeedsDraw();
            });
        };
    }

    public record TreeMetadata(Environment Environment, Process ParentProcess);
}

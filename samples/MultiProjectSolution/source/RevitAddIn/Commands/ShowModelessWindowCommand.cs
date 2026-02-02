using System.Windows;
using Autodesk.Revit.Attributes;
using ModelessModule.Views;
using Nice3point.Revit.Toolkit.External;

namespace RevitAddIn.Commands;

/// <summary>
///     External command entry point
/// </summary>
[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class ShowModelessWindowCommand : ExternalCommand
{
    public override void Execute()
    {
        var view = Host.GetService<ModelessModuleView>();

        if (view.WindowState == WindowState.Minimized)
        {
            view.WindowState = WindowState.Normal;
        }

        if (!view.IsVisible)
        {
            view.Show(Context.UiApplication.MainWindowHandle);
        }

        view.Focus();
    }
}
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using ModelessModule.Messages;
using ModelessModule.Models;
using ModelessModule.Services;
using Nice3point.Revit.Toolkit.External.Handlers;
using Nice3point.Revit.Toolkit.Options;

namespace ModelessModule.ViewModels;

public sealed partial class ModelessModuleViewModel(ElementMetadataExtractionService elementService, IMessenger messenger, ILogger<ModelessModuleViewModel> logger) : ObservableObject
{
    private readonly ActionEventHandler _externalHandler = new();
    private readonly AsyncEventHandler _asyncExternalHandler = new();
    private readonly AsyncEventHandler<ElementId> _asyncIdExternalHandler = new();

    [ObservableProperty] private ElementMetadata? _elementMetadata;
    [ObservableProperty] private string _status = string.Empty;

    [RelayCommand]
    private void ShowSummary()
    {
        _externalHandler.Raise(application =>
        {
            var selectionConfiguration = new SelectionConfiguration();
            var reference = application.ActiveUIDocument.Selection.PickObject(ObjectType.Element, selectionConfiguration.Filter);
            var element = reference.ElementId.ToElement(application.ActiveUIDocument.Document)!;

            ElementMetadata = elementService.ExtractMetadata(element);

            logger.LogInformation("Selection successful");
        });
    }

    [RelayCommand]
    private async Task DeleteElementAsync()
    {
        var deletedId = await _asyncIdExternalHandler.RaiseAsync(application =>
        {
            var document = application.ActiveUIDocument.Document;

            var selectionConfiguration = new SelectionConfiguration();
            var reference = application.ActiveUIDocument.Selection.PickObject(ObjectType.Element, selectionConfiguration.Filter);

            var transaction = new Transaction(document);
            transaction.Start("Delete element");
            document.Delete(reference.ElementId);
            transaction.Commit();

            return reference.ElementId;
        });

        TaskDialog.Show("Deleted element", $"ID: {deletedId}");
        logger.LogInformation("Deletion successful");
    }

    [RelayCommand]
    private async Task SelectDelayedElementAsync()
    {
        Status = "Wait 2 second...";
        await Task.Delay(TimeSpan.FromSeconds(2));

        await _asyncExternalHandler.RaiseAsync(application =>
        {
            messenger.Send(new SetWindowVisibilityMessage(false));

            var selectionConfiguration = new SelectionConfiguration();
            var reference = application.ActiveUIDocument.Selection.PickObject(ObjectType.Element, selectionConfiguration.Filter);
            var element = reference.ElementId.ToElement(application.ActiveUIDocument.Document)!;

            ElementMetadata = elementService.ExtractMetadata(element);

            logger.LogInformation("Selection successful");
            messenger.Send(new SetWindowVisibilityMessage(true));
        });

        Status = string.Empty;
    }
}
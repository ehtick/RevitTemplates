using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using ModelessModule.Messages;
using ModelessModule.ViewModels;

namespace ModelessModule.Views;

public sealed partial class ModelessModuleView : IRecipient<SetWindowVisibilityMessage>
{
    private readonly IMessenger _messenger;

    public ModelessModuleView(ModelessModuleViewModel viewModel, IMessenger messenger)
    {
        _messenger = messenger;
        DataContext = viewModel;
        InitializeComponent();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs args)
    {
        _messenger.Register(this);
    }

    private void OnUnloaded(object sender, RoutedEventArgs args)
    {
        _messenger.UnregisterAll(this);
    }

    public void Receive(SetWindowVisibilityMessage message)
    {
        if (message.Visible)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }
}
namespace ModelessModule.Messages;

/// <summary>
///     Message to control window visibility
/// </summary>
/// <param name="visible">Indicates whether the window should be visible</param>
public sealed class SetWindowVisibilityMessage(bool visible)
{
    public bool Visible { get; } = visible;
}

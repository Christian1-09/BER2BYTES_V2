namespace Ber2Bytes.Application.Common;

/// <summary>
/// Raised by the use cases whenever data changes, so the UI can refresh
/// things like the sidebar badges and the alerts bell.
/// </summary>
public class AppState
{
    public event Action? DataChanged;
    public void NotifyDataChanged() => DataChanged?.Invoke();
}

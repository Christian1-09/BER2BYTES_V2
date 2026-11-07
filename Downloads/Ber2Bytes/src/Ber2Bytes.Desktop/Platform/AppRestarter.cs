using System.Diagnostics;

namespace Ber2Bytes.Desktop.Platform;

public static class AppRestarter
{
    /// <summary>Starts a new copy of the app and closes this one (used after restoring a backup).</summary>
    public static void Restart()
    {
        var exe = Environment.ProcessPath;
        if (exe != null) Process.Start(exe);
        System.Windows.Application.Current.Dispatcher.Invoke(() => System.Windows.Application.Current.Shutdown());
    }
}

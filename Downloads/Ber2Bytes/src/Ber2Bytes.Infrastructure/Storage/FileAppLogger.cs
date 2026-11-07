namespace Ber2Bytes.Infrastructure.Storage;

/// <summary>Writes errors to %LocalAppData%\Ber2Bytes\error-log.txt.</summary>
public class FileAppLogger : IAppLogger
{
    private static readonly object Gate = new();

    public void Error(string message, Exception? ex = null)
    {
        try
        {
            lock (Gate)
            {
                File.AppendAllText(AppPaths.LogFile,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}");
            }
        }
        catch
        {
            // Logging must never crash the app.
        }
    }
}

using System.Diagnostics;
using Ber2Bytes.Application.Abstractions;
using Microsoft.Win32;

namespace Ber2Bytes.Desktop.Platform;

/// <summary>Windows Save/Open dialogs and opening files with their default program.</summary>
public class WpfFileDialogService(IAppLogger log) : IFileDialogService
{
    private static System.Windows.Application App => System.Windows.Application.Current;

    public string? AskSavePath(string defaultFileName, string filter)
    {
        return App.Dispatcher.Invoke(() =>
        {
            var dlg = new SaveFileDialog
            {
                FileName = defaultFileName,
                Filter = filter,
                AddExtension = true,
                OverwritePrompt = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            return dlg.ShowDialog(App.MainWindow) == true ? dlg.FileName : null;
        });
    }

    public string? AskOpenPath(string filter)
    {
        return App.Dispatcher.Invoke(() =>
        {
            var dlg = new OpenFileDialog { Filter = filter, CheckFileExists = true };
            return dlg.ShowDialog(App.MainWindow) == true ? dlg.FileName : null;
        });
    }

    public void OpenWithDefaultApp(string path)
    {
        try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); }
        catch (Exception ex) { log.Error("Could not open " + path, ex); }
    }

    public void ShowInFolder(string path)
    {
        try { Process.Start("explorer.exe", $"/select,\"{path}\""); }
        catch (Exception ex) { log.Error("Could not open folder " + path, ex); }
    }
}

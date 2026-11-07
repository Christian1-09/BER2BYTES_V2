using System.Windows;
using Ber2Bytes.Application.Abstractions;
using Ber2Bytes.Application.Features.Settings;
using Ber2Bytes.Infrastructure.Storage;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;

namespace Ber2Bytes.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        // Must be added before InitializeComponent so the BlazorWebView can find it.
        Resources.Add("services", App.Services);
        InitializeComponent();
        Closing += (_, _) => AutoBackupOnClose();
    }

    private void OnWebViewInitialized(object? sender, BlazorWebViewInitializedEventArgs e)
    {
        var core = e.WebView.CoreWebView2;

        // Product photos are saved as files in %LocalAppData%\Ber2Bytes\images.
        // This maps them to https://productimages.local/<file> so <img> tags can show them.
        core.SetVirtualHostNameToFolderMapping(AppPaths.ImageHost, AppPaths.ImagesFolder,
            CoreWebView2HostResourceAccessKind.Allow);

#if !DEBUG
        // Release build: behave like a desktop app, not a browser.
        core.Settings.AreBrowserAcceleratorKeysEnabled = false;
        core.Settings.IsZoomControlEnabled = false;
#endif
    }

    private static void AutoBackupOnClose()
    {
        try
        {
            var settings = App.Services.GetRequiredService<SettingsService>();
            if (settings.Current.AutoBackupOnExit)
                App.Services.GetRequiredService<IBackupService>().AutoBackup();
        }
        catch (Exception ex)
        {
            App.Services.GetRequiredService<IAppLogger>().Error("Auto backup on close failed", ex);
        }
    }
}

using System.Windows;
using System.Windows.Threading;
using Ber2Bytes.Application;
using Ber2Bytes.Application.Abstractions;
using Ber2Bytes.Desktop.Platform;
using Ber2Bytes.Desktop.UiServices;
using Ber2Bytes.Infrastructure;
using Ber2Bytes.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Ber2Bytes.Desktop;

// NOTE: "Application" alone would mean the Ber2Bytes.Application project namespace here,
// so the WPF class is always written as System.Windows.Application in this project.
public partial class App : System.Windows.Application
{
    /// <summary>The app-wide service container (MainWindow hands it to Blazor).</summary>
    public static IServiceProvider Services { get; private set; } = default!;

    /// <summary>
    /// COMPOSITION ROOT: the only place where all layers are wired together.
    /// Domain ← Application ← Infrastructure, and Desktop plugs in the Windows-specific services.
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        services.AddApplication();       // use cases (Ber2Bytes.Application)
        services.AddInfrastructure();    // SQLite, BCrypt, PDF/Excel, logging, backups (Ber2Bytes.Infrastructure)

        // Desktop / Windows implementations of Application interfaces
        services.AddSingleton<IImageStore, WpfImageStore>();
        services.AddSingleton<IFileDialogService, WpfFileDialogService>();

        // UI-only services
        services.AddSingleton<ToastService>();
        services.AddSingleton<ConfirmService>();
        services.AddSingleton<HotkeyService>();

        services.AddWpfBlazorWebView();
#if DEBUG
        services.AddBlazorWebViewDeveloperTools(); // F12 opens dev tools in Debug builds
#endif

        Services = services.BuildServiceProvider();
        var log = Services.GetRequiredService<IAppLogger>();

        DispatcherUnhandledException += OnUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            log.Error("Unhandled (domain)", args.ExceptionObject as Exception);

        try
        {
            Ber2Bytes.Infrastructure.DependencyInjection.InitializeDatabase(Services);
        }
        catch (Exception ex)
        {
            log.Error("Database initialization failed", ex);
            MessageBox.Show("The database could not be opened.\n\n" + ex.Message + $"\n\nDatabase file: {AppPaths.DatabaseFile}",
                "Ber2Bytes", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
            return;
        }

        var window = new MainWindow();
        MainWindow = window;
        window.Show();
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Services.GetRequiredService<IAppLogger>().Error("Unhandled (UI)", e.Exception);
        MessageBox.Show("Something went wrong:\n\n" + e.Exception.Message + $"\n\nDetails were saved to:\n{AppPaths.LogFile}",
            "Ber2Bytes", MessageBoxButton.OK, MessageBoxImage.Warning);
        e.Handled = true; // keep the app running
    }
}

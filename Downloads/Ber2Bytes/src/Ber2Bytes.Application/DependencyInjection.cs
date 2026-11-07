using Microsoft.Extensions.DependencyInjection;

namespace Ber2Bytes.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers all use-case services. They are singletons because a desktop app has one window
    /// and one signed-in user at a time; each operation opens its own short-lived database context.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<AppState>();
        services.AddSingleton<AuditService>();
        services.AddSingleton<AuthService>();
        services.AddSingleton<SettingsService>();
        services.AddSingleton<CatalogService>();
        services.AddSingleton<InventoryService>();
        services.AddSingleton<PosCart>();
        services.AddSingleton<SalesService>();
        services.AddSingleton<ReturnService>();
        services.AddSingleton<ReportService>();
        services.AddSingleton<ExportService>();
        services.AddSingleton<DashboardService>();
        services.AddSingleton<AlertService>();
        services.AddSingleton<UserService>();
        return services;
    }
}

using Ber2Bytes.Infrastructure.Export;
using Ber2Bytes.Infrastructure.Persistence;
using Ber2Bytes.Infrastructure.Security;
using Ber2Bytes.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ber2Bytes.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers the database and the other technical services behind the Application interfaces.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        AppPaths.EnsureFolders();

        // QuestPDF is free for small businesses under the Community license.
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        services.AddDbContextFactory<AppDbContext>(o => o.UseSqlite($"Data Source={AppPaths.DatabaseFile}"));
        services.AddSingleton<IAppDbContextFactory, AppDbContextFactory>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IAppLogger, FileAppLogger>();
        services.AddSingleton<IUserPreferences, JsonUserPreferences>();
        services.AddSingleton<IReportWriter, ReportWriter>();
        services.AddSingleton<IBackupService, BackupService>();
        return services;
    }

    /// <summary>Creates the database file and tables on first run.</summary>
    public static void InitializeDatabase(IServiceProvider services) =>
        DatabaseInitializer.Initialize(services.GetRequiredService<IDbContextFactory<AppDbContext>>());
}

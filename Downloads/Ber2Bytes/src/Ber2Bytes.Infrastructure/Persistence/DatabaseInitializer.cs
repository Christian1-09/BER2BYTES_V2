using Microsoft.EntityFrameworkCore;

namespace Ber2Bytes.Infrastructure.Persistence;

/// <summary>
/// Creates the SQLite database on first run. The store starts EMPTY:
/// no accounts, categories, suppliers or products. On first launch the app shows the
/// one-time administrator registration screen (see AuthService.RegisterAdminAsync).
/// </summary>
public static class DatabaseInitializer
{
    public static void Initialize(IDbContextFactory<AppDbContext> factory)
    {
        using var db = factory.CreateDbContext();

        // EnsureCreated is simple and works offline without tools. If you change the entity
        // classes after the database exists, delete the .db file while developing, or switch
        // to EF Core Migrations before going live (see README).
        db.Database.EnsureCreated();
        db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");

        if (!db.Settings.Any())
        {
            db.Settings.Add(new StoreSettings());
            db.SaveChanges();
        }
    }
}

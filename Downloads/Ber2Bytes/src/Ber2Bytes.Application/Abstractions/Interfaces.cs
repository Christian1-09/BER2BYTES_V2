using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ber2Bytes.Application.Abstractions;

// ──────────────────────────────────────────────────────────────────────────────
// Interfaces the Application layer needs. Implementations live in:
//   Infrastructure → database, password hashing, PDF/Excel, logging, preferences, backups
//   Desktop        → Windows file dialogs, product photo resizing (WPF imaging)
// ──────────────────────────────────────────────────────────────────────────────

/// <summary>The database as the use cases see it (implemented by the SQLite AppDbContext).</summary>
public interface IAppDbContext : IDisposable, IAsyncDisposable
{
    DbSet<User> Users { get; }
    DbSet<Category> Categories { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<Product> Products { get; }
    DbSet<Sale> Sales { get; }
    DbSet<SaleItem> SaleItems { get; }
    DbSet<StockMovement> StockMovements { get; }
    DbSet<ReturnRequest> Returns { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<StoreSettings> Settings { get; }

    DatabaseFacade Database { get; }
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>Creates a short-lived database context for one operation.</summary>
public interface IAppDbContextFactory
{
    IAppDbContext Create();
    Task<IAppDbContext> CreateAsync();
}

public interface IPasswordHasher
{
    string Hash(string secret);
    bool Verify(string secret, string hash);
}

public interface IAppLogger
{
    void Error(string message, Exception? ex = null);
}

/// <summary>Settings saved per computer (not in the database).</summary>
public interface IUserPreferences
{
    string? RememberedUsername { get; set; }
    bool SidebarCollapsed { get; set; }
    void Save();
}

/// <summary>Saves and deletes product photos.</summary>
public interface IImageStore
{
    const long MaxUploadBytes = 8 * 1024 * 1024;
    /// <summary>Resizes and saves the photo. Returns the stored file name.</summary>
    string Save(byte[] imageData);
    void Delete(string? fileName);
}

/// <summary>Windows Save/Open dialogs and opening files.</summary>
public interface IFileDialogService
{
    string? AskSavePath(string defaultFileName, string filter);
    string? AskOpenPath(string filter);
    void OpenWithDefaultApp(string path);
    void ShowInFolder(string path);
}

/// <summary>Writes a report as PDF or Excel.</summary>
public interface IReportWriter
{
    void WritePdf(Features.Reports.ReportResult report, Features.Reports.ReportHeader header, string path);
    void WriteExcel(Features.Reports.ReportResult report, Features.Reports.ReportHeader header, string path);
}

/// <summary>Backup = a .zip with the database and the product photos.</summary>
public interface IBackupService
{
    void CreateBackup(string zipPath);
    /// <summary>Returns an error message, or null when the file is a valid backup.</summary>
    string? Validate(string zipPath);
    /// <summary>Replaces the current data with the backup. The app must restart afterwards.</summary>
    void Restore(string zipPath);
    /// <summary>Daily automatic backup into the auto-backups folder (keeps the last 14).</summary>
    void AutoBackup();
    string DataFolder { get; }
    string AutoBackupFolder { get; }
}

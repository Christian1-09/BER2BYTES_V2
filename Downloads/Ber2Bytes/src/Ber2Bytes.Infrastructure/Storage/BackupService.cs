using System.IO.Compression;
using Microsoft.Data.Sqlite;

namespace Ber2Bytes.Infrastructure.Storage;

/// <summary>Backup = a .zip with the SQLite database and the product photos.</summary>
public class BackupService(IAppLogger log) : IBackupService
{
    private const string DbEntryName = "ber2bytes-data.db";

    public string DataFolder => AppPaths.DataFolder;
    public string AutoBackupFolder => AppPaths.AutoBackupFolder;

    public void CreateBackup(string zipPath)
    {
        var tempDb = Path.Combine(Path.GetTempPath(), $"b2b-backup-{Guid.NewGuid():N}.db");
        try
        {
            // SQLite's online backup API makes a consistent copy even while the app is running.
            using (var source = new SqliteConnection($"Data Source={AppPaths.DatabaseFile}"))
            using (var target = new SqliteConnection($"Data Source={tempDb};Pooling=False"))
            {
                source.Open();
                target.Open();
                source.BackupDatabase(target);
            }

            if (File.Exists(zipPath)) File.Delete(zipPath);
            using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
            zip.CreateEntryFromFile(tempDb, DbEntryName);
            foreach (var img in Directory.GetFiles(AppPaths.ImagesFolder))
                zip.CreateEntryFromFile(img, "images/" + Path.GetFileName(img));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(tempDb)) File.Delete(tempDb);
        }
    }

    public string? Validate(string zipPath)
    {
        try
        {
            using var zip = ZipFile.OpenRead(zipPath);
            return zip.GetEntry(DbEntryName) == null ? "This file is not a Ber2Bytes backup." : null;
        }
        catch
        {
            return "This file is not a valid .zip backup.";
        }
    }

    /// <summary>Replaces the database and photos with the backup. A safety copy of the current data is saved first.</summary>
    public void Restore(string zipPath)
    {
        CreateBackup(Path.Combine(AppPaths.AutoBackupFolder, $"before-restore-{DateTime.Now:yyyyMMdd-HHmmss}.zip"));
        SqliteConnection.ClearAllPools();

        var extractDir = Path.Combine(Path.GetTempPath(), $"b2b-restore-{Guid.NewGuid():N}");
        ZipFile.ExtractToDirectory(zipPath, extractDir);

        foreach (var f in new[] { AppPaths.DatabaseFile, AppPaths.DatabaseFile + "-wal", AppPaths.DatabaseFile + "-shm" })
            if (File.Exists(f)) File.Delete(f);
        File.Copy(Path.Combine(extractDir, DbEntryName), AppPaths.DatabaseFile, true);

        foreach (var img in Directory.GetFiles(AppPaths.ImagesFolder)) File.Delete(img);
        var imgDir = Path.Combine(extractDir, "images");
        if (Directory.Exists(imgDir))
            foreach (var img in Directory.GetFiles(imgDir))
                File.Copy(img, Path.Combine(AppPaths.ImagesFolder, Path.GetFileName(img)), true);

        Directory.Delete(extractDir, true);
    }

    public void AutoBackup()
    {
        try
        {
            if (!File.Exists(AppPaths.DatabaseFile)) return;
            CreateBackup(Path.Combine(AppPaths.AutoBackupFolder, $"auto-{DateTime.Now:yyyy-MM-dd}.zip"));
            foreach (var old in Directory.GetFiles(AppPaths.AutoBackupFolder, "auto-*.zip").OrderByDescending(f => f).Skip(14))
                File.Delete(old);
        }
        catch (Exception ex) { log.Error("Auto backup failed", ex); }
    }
}

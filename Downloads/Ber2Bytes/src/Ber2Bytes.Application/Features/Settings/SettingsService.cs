namespace Ber2Bytes.Application.Features.Settings;

/// <summary>Store settings, cached in memory after the first read.</summary>
public class SettingsService(IAppDbContextFactory dbf, AuthService auth, AppState state)
{
    private StoreSettings? _cache;

    public StoreSettings Current
    {
        get
        {
            if (_cache == null)
            {
                using var db = dbf.Create();
                _cache = db.Settings.AsNoTracking().First();
            }
            return _cache;
        }
    }

    public async Task<string?> SaveAsync(StoreSettings s)
    {
        if (string.IsNullOrWhiteSpace(s.StoreName)) return "Store name is required.";
        if (s.StaffMaxDiscountPercent is < 0 or > 100) return "Staff discount limit must be between 0 and 100%.";
        if (s.DeadStockDays < 7) return "Dead stock days must be at least 7.";
        if (s.SlowMovingMaxQty < 0) return "Slow-moving quantity cannot be negative.";
        if (s.DefaultReorderLevel < 0) return "Reorder level cannot be negative.";
        if (s.ReturnWindowDays < 0) return "Return window cannot be negative.";

        await using var db = await dbf.CreateAsync();
        var row = await db.Settings.FirstAsync();
        db.Entry(row).CurrentValues.SetValues(s);
        row.Id = 1;
        AuditService.Add(db, auth.UserId, auth.UserName, "Settings", "Store settings updated");
        await db.SaveChangesAsync();
        _cache = null;
        state.NotifyDataChanged();
        return null;
    }

    public void ClearCache() => _cache = null;

    /// <summary>
    /// "Start fresh": deletes all sales, returns, stock movements and the activity log.
    /// Products, categories, suppliers, users and settings are kept. Admin only.
    /// </summary>
    public async Task<string?> ClearTransactionsAsync()
    {
        if (!auth.Can(Perm.Settings)) return "Only an administrator can do this.";
        await using var db = await dbf.CreateAsync();
        await using var tx = await db.Database.BeginTransactionAsync();
        await db.Returns.ExecuteDeleteAsync();
        await db.SaleItems.ExecuteDeleteAsync();
        await db.Sales.ExecuteDeleteAsync();
        await db.StockMovements.ExecuteDeleteAsync();
        await db.AuditLogs.ExecuteDeleteAsync();
        AuditService.Add(db, auth.UserId, auth.UserName, "Start fresh", "All transactions were cleared");
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        state.NotifyDataChanged();
        return null;
    }

    /// <summary>A copy the Settings page can edit without changing the cache until saved.</summary>
    public StoreSettings EditableCopy()
    {
        var c = Current;
        return new StoreSettings
        {
            Id = 1, StoreName = c.StoreName, BranchName = c.BranchName, Address = c.Address, Phone = c.Phone,
            ReceiptFooter = c.ReceiptFooter, DefaultReorderLevel = c.DefaultReorderLevel,
            StaffMaxDiscountPercent = c.StaffMaxDiscountPercent, DeadStockDays = c.DeadStockDays,
            SlowMovingMaxQty = c.SlowMovingMaxQty, ReturnWindowDays = c.ReturnWindowDays, AutoBackupOnExit = c.AutoBackupOnExit
        };
    }
}

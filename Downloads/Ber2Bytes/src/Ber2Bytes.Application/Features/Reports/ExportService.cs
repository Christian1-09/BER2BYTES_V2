namespace Ber2Bytes.Application.Features.Reports;

/// <summary>Store details printed at the top of every exported report.</summary>
public record ReportHeader(string StoreName, string BranchName, string? Address, string? Phone, string GeneratedBy, DateTime GeneratedAt);

/// <summary>
/// Use case "export a report": asks where to save (IFileDialogService), writes the file
/// (IReportWriter = QuestPDF / ClosedXML in Infrastructure), logs it and opens it.
/// </summary>
public class ExportService(SettingsService settings, AuthService auth, IFileDialogService files, IReportWriter writer, AuditService audit)
{
    /// <summary>Saves the report as PDF and opens it (to print). Returns the path, or null if cancelled.</summary>
    public async Task<string?> SavePdfAsync(ReportResult r)
    {
        var path = files.AskSavePath($"{Fmt.FileSafe(r.Title)}_{DateTime.Now:yyyy-MM-dd}.pdf", "PDF file (*.pdf)|*.pdf");
        if (path == null) return null;
        var header = Header();
        await Task.Run(() => writer.WritePdf(r, header, path));
        await audit.LogAsync(auth.UserId, auth.UserName, "Export PDF", r.Title);
        files.OpenWithDefaultApp(path);
        return path;
    }

    public async Task<string?> SaveExcelAsync(ReportResult r)
    {
        var path = files.AskSavePath($"{Fmt.FileSafe(r.Title)}_{DateTime.Now:yyyy-MM-dd}.xlsx", "Excel workbook (*.xlsx)|*.xlsx");
        if (path == null) return null;
        var header = Header();
        await Task.Run(() => writer.WriteExcel(r, header, path));
        await audit.LogAsync(auth.UserId, auth.UserName, "Export Excel", r.Title);
        files.ShowInFolder(path);
        return path;
    }

    private ReportHeader Header()
    {
        var s = settings.Current;
        return new ReportHeader(s.StoreName, s.BranchName, s.Address, s.Phone, auth.UserName, DateTime.Now);
    }

    /// <summary>Formats one report cell the same way on screen, in PDF and in Excel text.</summary>
    public static string FormatCell(object? v, ColKind kind) => v switch
    {
        null => "",
        decimal d when kind == ColKind.Money => Fmt.Peso(d),
        decimal d => d.ToString("N2", Fmt.PH),
        int i => i.ToString("N0", Fmt.PH),
        DateTime dt when kind == ColKind.Date && dt.TimeOfDay == TimeSpan.Zero => Fmt.Date(dt),
        DateTime dt => Fmt.DateTimeText(dt),
        _ => v.ToString() ?? ""
    };

    public static bool IsRightAligned(ColKind k) => k is ColKind.Money or ColKind.Number;

    /// <summary>The product list (Products page → Export Excel).</summary>
    public static ReportResult ProductList(List<Product> products, bool showCost)
    {
        var r = new ReportResult
        {
            Title = "Product List", Subtitle = $"{products.Count} products as of {DateTime.Now:MMM d, yyyy}", Landscape = true,
            Columns = [new("SKU", ColKind.Mono, 1.2f), new("Product", ColKind.Text, 2.8f), new("Category", ColKind.Text, 1.2f),
                       new("Brand"), new("Supplier", ColKind.Text, 1.4f)]
        };
        if (showCost) r.Columns.Add(new("Cost", ColKind.Money));
        r.Columns.AddRange([new("Price", ColKind.Money), new("Stock", ColKind.Number, .6f), new("Reorder", ColKind.Number, .6f), new("Status", ColKind.Status, .9f)]);

        foreach (var p in products)
        {
            var row = new List<object?> { p.Sku, p.Name, p.Category?.Name, p.Brand, p.Supplier?.Name };
            if (showCost) row.Add(p.CostPrice);
            row.AddRange([p.SellingPrice, p.Stock, p.ReorderLevel, p.StockStatus]);
            r.Rows.Add(row.ToArray());
        }
        return r;
    }
}

namespace Ber2Bytes.Application.Security;

/// <summary>
/// Every action in the app is checked against one of these permissions.
/// Admin has all of them. Staff has only the ones in <see cref="AuthService.StaffPermissions"/>.
/// </summary>
public static class Perm
{
    public const string Dashboard = "dashboard";
    public const string Pos = "pos";
    public const string ProductsView = "products.view";
    public const string ProductsManage = "products.manage";     // add / edit / archive / delete
    public const string ViewCost = "products.cost";             // see cost price and profit
    public const string StockView = "stock.view";
    public const string StockIn = "stock.in";
    public const string StockAdjust = "stock.adjust";
    public const string SalesView = "sales.view";
    public const string SalesViewAll = "sales.all";             // staff only sees their own sales
    public const string SalesVoid = "sales.void";
    public const string Categories = "categories";
    public const string SuppliersView = "suppliers.view";
    public const string SuppliersManage = "suppliers.manage";
    public const string Returns = "returns";
    public const string ReturnsProcess = "returns.process";     // approve / reject
    public const string Reports = "reports";
    public const string ReportLowStock = "report.lowstock";
    public const string ReportBusiness = "report.business";     // sales, top sellers, dead stock, valuation
    public const string Alerts = "alerts";
    public const string Users = "users";
    public const string Audit = "audit";
    public const string Settings = "settings";
}

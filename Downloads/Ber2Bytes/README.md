# Ber2Bytes — Automated Inventory and Sales Tracking System

Offline Windows desktop app for **Ber2Bytes Computer Parts & Accessories (Dipolog City Branch)**.
Built with **.NET 10 · Blazor Hybrid on WPF · SQLite (EF Core)**, organised with **Clean Architecture**.

---

## 1. What you need

| | |
|---|---|
| Windows | 10 (version 1809 or newer) or 11, 64-bit |
| Visual Studio | **Visual Studio 2026** (or newer) with the workloads **.NET desktop development** and **ASP.NET and web development** |
| .NET SDK | **.NET 10** (installed with Visual Studio) |
| WebView2 Runtime | Already in Windows 11 / updated Windows 10. If the window stays blank, install "Microsoft Edge WebView2 Runtime". |

The first build needs internet **once** to download the NuGet packages. After that the app runs fully offline.

## 2. Run it

1. Unzip and open **`Ber2Bytes.sln`**.
2. Make sure **Ber2Bytes.Desktop** is the startup project (right-click it → *Set as Startup Project*).
3. **Build → Rebuild Solution**, then press **F5**.

### First launch: register the administrator (one time only)
On a fresh install the app opens a **"Create the Administrator Account"** screen. Enter:
- Full name, username, password (+ confirm)
- A **security question** (choose one or write your own) and its **answer** — used for *Forgot password?*

The account is created and you are signed in. **This screen never appears again**: once any account exists, registration is closed for good (the check is done again inside the database transaction, so it cannot be bypassed).

The store starts **empty**. The dashboard shows a *Getting started* guide: add categories → suppliers → products → staff accounts.

### Forgot password
On the login screen click **Forgot password?** → enter the username → answer the security question → set a new password.
- Every user has a security question. Staff accounts created by the admin are asked to choose one the **first time they sign in**.
- Anyone can change theirs later from the user menu (top-right) → **Security question**.
- 5 wrong answers lock recovery for 60 seconds. Answers ignore upper/lower case and extra spaces.
- An admin can still reset any password in **User Management → Reset password**.

## 3. Your logo and login picture

Put these files in **`src/Ber2Bytes.Desktop/wwwroot/images/`**, then rebuild:

| File | Used for |
|---|---|
| `logo.png` | Logo on the white side of the login (transparent PNG, ~600 px wide) |
| `logo-dark.png` | *(optional)* Logo for black backgrounds: sidebar and left side of the login |
| `login-hero.jpg` | Picture on the left side of the login (~1200×1400 px) |
| `app.ico` | *(optional)* Window icon. Then uncomment `<ApplicationIcon>` in `Ber2Bytes.Desktop.csproj` |

## 4. Clean Architecture

```
            ┌──────────────────────────────────────────┐
            │  Ber2Bytes.Desktop   (Presentation)      │  WPF + Blazor screens, composition root
            └──────────────┬───────────────┬───────────┘
                           │               │
                           ▼               ▼
┌──────────────────────────────┐   ┌───────────────────────────────┐
│ Ber2Bytes.Application        │◄──│ Ber2Bytes.Infrastructure      │  SQLite/EF Core, BCrypt,
│ (Use cases + interfaces)     │   │ (implements the interfaces)   │  QuestPDF, ClosedXML, backups
└──────────────┬───────────────┘   └───────────────────────────────┘
               ▼
┌──────────────────────────────┐
│ Ber2Bytes.Domain             │  Entities and enums – depends on nothing
└──────────────────────────────┘
```

**The rule:** arrows point inward. Domain knows nothing else; Application only knows Domain and its own
interfaces; Infrastructure and Desktop *implement* those interfaces. So you could swap SQLite for SQL Server,
or WPF for another UI, without touching the business rules.

| Project | Contains |
|---|---|
| **Ber2Bytes.Domain** | `Entities/` User, Category, Supplier, Product, Sale, SaleItem, StockMovement, ReturnRequest, AuditLog, StoreSettings · `Enums/` · simple business rules (e.g. `Product.IsLowStock`, `Sale.GrossProfit`) |
| **Ber2Bytes.Application** | `Abstractions/Interfaces.cs` — IAppDbContext, IPasswordHasher, IImageStore, IFileDialogService, IReportWriter, IBackupService, IUserPreferences, IAppLogger · `Features/` — one folder per use case: **Auth** (registration, login, lockout, forgot password), Catalog, Inventory, Sales (POS cart, checkout, void), Returns, Reports (+ export), Dashboard (+ alerts), Users, Audit, Settings · `Security/` permissions, password rules, security questions · `Common/` formatting |
| **Ber2Bytes.Infrastructure** | `Persistence/` AppDbContext (SQLite), factory, DatabaseInitializer · `Security/` BCrypt hasher · `Export/` PDF + Excel writer · `Storage/` data paths, error log, prefs.json, backup/restore |
| **Ber2Bytes.Desktop** | `App.xaml.cs` (**composition root** — wires all layers) · `MainWindow` (WebView) · `Platform/` Windows-only implementations: file dialogs, photo resizing, restart · `UiServices/` toasts, confirm dialogs, keyboard shortcuts · `Components/` all screens · `wwwroot/` CSS, icons, fonts, images |

Every action checks a permission from `Perm` (`Application/Security/Perm.cs`). Staff permissions are listed in `AuthService.StaffPermissions`.

## 5. Features

**Both roles**
- **Point of Sale**: search by product name or SKU (Enter adds an exact SKU match), photos, category filter, cart, discount (% or ₱), **Cash or GCash**, quick-cash buttons, change, **hold/recall**, printable receipt. Shortcuts: **F2** search, **F4** cash/ref box, **F9** charge, **Esc** close dialog, **Ctrl+K** global search.
- Dashboard (charts, best sellers, slow-moving, low stock), Products (view), Inventory with **Stock In**, Suppliers (view), Returns, Alerts.
- **Low Stock / Reorder List** per supplier → **PDF** or **Excel**.

**Administrator only**: product add/edit/archive with photos and cost/profit · stock adjustments · all sales and voids · approve returns · reports (Sales, Top-Selling, **Slow-Moving & Dead Stock**, Inventory Valuation) · categories · suppliers · user accounts · activity log · settings · backup/restore.

**Not included (by request):** VAT · card payment · serial numbers/warranty.

## 6. Where the data is

`%LocalAppData%\Ber2Bytes\` (e.g. `C:\Users\<you>\AppData\Local\Ber2Bytes\`)

| | |
|---|---|
| `ber2bytes-data.db` | SQLite database |
| `images\` | Product photos |
| `auto-backups\` | Daily automatic backups (last 14 kept) |
| `error-log.txt` | Error details |
| `prefs.json` | "Remember me" and sidebar setting |

> Version 2 uses a **new database file name**, so data from the earlier test version is not reused and the registration screen appears. To start over during testing, close the app and delete `ber2bytes-data.db` (the registration screen will show again).

**Backups:** Settings → *Create Backup…* (a `.zip` with database + photos). *Restore Backup…* replaces everything and restarts.

## 7. Changing the database tables later

The app uses `EnsureCreated()` (no tools needed). If you change entity classes:
- **While developing:** delete `ber2bytes-data.db` and run again.
- **After going live:** switch to EF Core Migrations so existing data is kept.

## 8. Install on the store computer

```
dotnet publish src/Ber2Bytes.Desktop -c Release -r win-x64 --self-contained true -o publish
```
Copy the `publish` folder to the store PC and create a shortcut to `Ber2Bytes.exe`.

## 9. Licenses

QuestPDF: free **Community license** (businesses under USD 1M annual revenue). Montserrat font: SIL Open Font License.

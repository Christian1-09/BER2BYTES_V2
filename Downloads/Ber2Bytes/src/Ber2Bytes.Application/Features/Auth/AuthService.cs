using System.Text.RegularExpressions;

namespace Ber2Bytes.Application.Features.Auth;

public enum LoginResult { Success, InvalidCredentials, Disabled, LockedOut }

public class RegisterAdminForm
{
    public string FullName { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
    public string SecurityQuestion { get; set; } = "";
    public string SecurityAnswer { get; set; } = "";
}

/// <summary>
/// Sign-in, the one-time administrator registration, the signed-in user and their permissions,
/// and password recovery with a security question.
/// </summary>
public class AuthService(IAppDbContextFactory dbf, AuditService audit, IUserPreferences prefs, IPasswordHasher hasher)
{
    public static readonly HashSet<string> StaffPermissions =
    [
        Perm.Dashboard, Perm.Pos, Perm.ProductsView, Perm.StockView, Perm.StockIn,
        Perm.SalesView, Perm.SuppliersView, Perm.Returns, Perm.Reports, Perm.ReportLowStock, Perm.Alerts
    ];

    private const int MaxAttempts = 5;
    private static readonly TimeSpan LockTime = TimeSpan.FromSeconds(60);
    private int _failedAttempts;          // wrong passwords
    private DateTime? _lockedUntil;
    private int _failedAnswers;           // wrong security answers (counted separately)
    private DateTime? _recoveryLockedUntil;

    public User? CurrentUser { get; private set; }
    public bool IsLoggedIn => CurrentUser != null;
    public bool IsAdmin => CurrentUser?.Role == UserRole.Admin;
    public int UserId => CurrentUser?.Id ?? 0;
    public string UserName => CurrentUser?.FullName ?? "";
    /// <summary>True when the signed-in user still has to choose a security question.</summary>
    public bool NeedsSecurityQuestion => CurrentUser != null && !CurrentUser.HasSecurityQuestion;
    public event Action? Changed;

    public bool Can(string permission) =>
        CurrentUser != null && (CurrentUser.Role == UserRole.Admin || StaffPermissions.Contains(permission));

    public int LockSecondsLeft => SecondsLeft(_lockedUntil);
    public int RecoveryLockSecondsLeft => SecondsLeft(_recoveryLockedUntil);

    private static int SecondsLeft(DateTime? until) =>
        until is { } t && t > DateTime.Now ? (int)Math.Ceiling((t - DateTime.Now).TotalSeconds) : 0;

    // ───────────────────────── First-run registration ─────────────────────────

    /// <summary>True on a fresh install: no account exists yet, so the administrator must register.</summary>
    public async Task<bool> IsSetupRequiredAsync()
    {
        await using var db = await dbf.CreateAsync();
        return !await db.Users.AnyAsync();
    }

    /// <summary>
    /// Creates the first (administrator) account and signs in. This only works once:
    /// as soon as any account exists, registration is closed for good.
    /// </summary>
    public async Task<string?> RegisterAdminAsync(RegisterAdminForm f)
    {
        f.FullName = f.FullName.Trim();
        f.Username = f.Username.Trim().ToLowerInvariant();
        if (f.FullName.Length == 0) return "Please enter your full name.";
        if (!IsValidUsername(f.Username)) return "Username must be 3–30 characters: letters, numbers, dot or underscore.";
        if (PasswordRules.Validate(f.Password, f.ConfirmPassword) is { } pwError) return pwError;
        if (SecurityQuestions.Validate(f.SecurityQuestion, f.SecurityAnswer) is { } qError) return qError;

        await using var db = await dbf.CreateAsync();
        await using var tx = await db.Database.BeginTransactionAsync();
        if (await db.Users.AnyAsync())
            return "The administrator account was already registered. Please sign in instead.";

        var admin = new User
        {
            FullName = f.FullName,
            Username = f.Username,
            Role = UserRole.Admin,
            IsActive = true,
            PasswordHash = hasher.Hash(f.Password),
            SecurityQuestion = f.SecurityQuestion.Trim(),
            SecurityAnswerHash = hasher.Hash(SecurityQuestions.Normalize(f.SecurityAnswer)),
            LastLoginAt = DateTime.Now
        };
        db.Users.Add(admin);
        AuditService.Add(db, null, admin.FullName, "Registration", $"Administrator account '{admin.Username}' created on first run");
        await db.SaveChangesAsync();
        await tx.CommitAsync();

        CurrentUser = admin;
        prefs.RememberedUsername = admin.Username;
        prefs.Save();
        Changed?.Invoke();
        return null;
    }

    public static bool IsValidUsername(string username) => Regex.IsMatch(username, "^[a-z0-9._]{3,30}$");

    // ───────────────────────── Sign in / out ─────────────────────────

    public async Task<LoginResult> LoginAsync(string username, string password, bool rememberMe)
    {
        if (LockSecondsLeft > 0) return LoginResult.LockedOut;

        username = username.Trim().ToLowerInvariant();
        await using var db = await dbf.CreateAsync();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user == null || !hasher.Verify(password, user.PasswordHash))
        {
            if (RegisterFailure())
            {
                await audit.LogAsync(null, username, "Login locked", $"Too many failed sign-in attempts for '{username}'");
                return LoginResult.LockedOut;
            }
            return LoginResult.InvalidCredentials;
        }
        if (!user.IsActive) return LoginResult.Disabled;

        _failedAttempts = 0;
        user.LastLoginAt = DateTime.Now;
        await db.SaveChangesAsync();
        CurrentUser = user;

        prefs.RememberedUsername = rememberMe ? user.Username : null;
        prefs.Save();

        await audit.LogAsync(user.Id, user.FullName, "Login", "Signed in");
        Changed?.Invoke();
        return LoginResult.Success;
    }

    public async Task LogoutAsync()
    {
        if (CurrentUser != null)
            await audit.LogAsync(CurrentUser.Id, CurrentUser.FullName, "Logout", "Signed out");
        CurrentUser = null;
        Changed?.Invoke();
    }

    /// <summary>Counts a failed attempt. Returns true when the user is now locked out.</summary>
    private bool RegisterFailure()
    {
        _failedAttempts++;
        if (_failedAttempts < MaxAttempts) return false;
        _lockedUntil = DateTime.Now.Add(LockTime);
        _failedAttempts = 0;
        return true;
    }

    // ───────────────────────── Forgot password (security question) ─────────────────────────

    /// <summary>Step 1: returns the user's security question, or an error message.</summary>
    public async Task<(string? Question, string? Error)> GetSecurityQuestionAsync(string username)
    {
        if (RecoveryLockSecondsLeft > 0) return (null, $"Too many wrong answers. Please wait {RecoveryLockSecondsLeft} seconds.");
        username = username.Trim().ToLowerInvariant();
        if (username.Length == 0) return (null, "Please enter your username.");

        await using var db = await dbf.CreateAsync();
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return (null, "No account was found with that username.");
        if (!user.HasSecurityQuestion)
            return (null, "This account has no security question yet. Please ask your administrator to reset your password.");
        return (user.SecurityQuestion, null);
    }

    /// <summary>Step 2: checks the answer and sets a new password. Returns an error message or null.</summary>
    public async Task<string?> ResetPasswordWithAnswerAsync(string username, string answer, string newPassword, string confirmPassword)
    {
        if (RecoveryLockSecondsLeft > 0) return $"Too many wrong answers. Please wait {RecoveryLockSecondsLeft} seconds.";
        if (PasswordRules.Validate(newPassword, confirmPassword) is { } pwError) return pwError;

        username = username.Trim().ToLowerInvariant();
        await using var db = await dbf.CreateAsync();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null || !user.HasSecurityQuestion) return "This account cannot be recovered with a security question.";
        if (!user.IsActive) return "This account is disabled. Please contact your administrator.";

        if (!hasher.Verify(SecurityQuestions.Normalize(answer), user.SecurityAnswerHash!))
        {
            if (++_failedAnswers >= MaxAttempts)
            {
                _failedAnswers = 0;
                _recoveryLockedUntil = DateTime.Now.Add(LockTime);
                await audit.LogAsync(user.Id, user.FullName, "Recovery locked", "Too many wrong security answers");
                return $"Too many wrong answers. Please wait {RecoveryLockSecondsLeft} seconds.";
            }
            return "That answer is not correct.";
        }

        _failedAnswers = 0;
        user.PasswordHash = hasher.Hash(newPassword);
        AuditService.Add(db, user.Id, user.FullName, "Password recovered", "Password reset using the security question");
        await db.SaveChangesAsync();
        return null;
    }

    // ───────────────────────── Signed-in user's own account ─────────────────────────

    public async Task<string?> ChangeOwnPasswordAsync(string current, string newPassword, string confirm)
    {
        if (CurrentUser == null) return "You are not signed in.";
        if (PasswordRules.Validate(newPassword, confirm) is { } pwError) return pwError;
        await using var db = await dbf.CreateAsync();
        var user = await db.Users.FindAsync(CurrentUser.Id);
        if (user == null || !hasher.Verify(current, user.PasswordHash)) return "Your current password is incorrect.";
        user.PasswordHash = hasher.Hash(newPassword);
        AuditService.Add(db, user.Id, user.FullName, "Change password", "Changed own password");
        await db.SaveChangesAsync();
        return null;
    }

    /// <summary>Sets or changes the signed-in user's security question. The current password is required.</summary>
    public async Task<string?> SetSecurityQuestionAsync(string currentPassword, string question, string answer)
    {
        if (CurrentUser == null) return "You are not signed in.";
        if (SecurityQuestions.Validate(question, answer) is { } qError) return qError;
        await using var db = await dbf.CreateAsync();
        var user = await db.Users.FindAsync(CurrentUser.Id);
        if (user == null || !hasher.Verify(currentPassword, user.PasswordHash)) return "Your password is incorrect.";
        user.SecurityQuestion = question.Trim();
        user.SecurityAnswerHash = hasher.Hash(SecurityQuestions.Normalize(answer));
        AuditService.Add(db, user.Id, user.FullName, "Security question", "Security question set");
        await db.SaveChangesAsync();
        CurrentUser = user;
        Changed?.Invoke();
        return null;
    }

    /// <summary>Called after an admin edits their own account so the sidebar shows the new name.</summary>
    public void RefreshCurrentUser(User updated)
    {
        if (CurrentUser?.Id == updated.Id)
        {
            CurrentUser = updated;
            Changed?.Invoke();
        }
    }
}

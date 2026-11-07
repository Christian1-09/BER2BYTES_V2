using Ber2Bytes.Domain.Enums;

namespace Ber2Bytes.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public UserRole Role { get; set; } = UserRole.Staff;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? LastLoginAt { get; set; }

    /// <summary>Question shown on "Forgot password?" (null until the user sets one).</summary>
    public string? SecurityQuestion { get; set; }

    /// <summary>The answer is hashed like a password; it is never stored as plain text.</summary>
    public string? SecurityAnswerHash { get; set; }

    public bool HasSecurityQuestion => !string.IsNullOrEmpty(SecurityQuestion) && !string.IsNullOrEmpty(SecurityAnswerHash);
    public bool IsAdmin => Role == UserRole.Admin;
}

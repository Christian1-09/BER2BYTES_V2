namespace Ber2Bytes.Application.Security;

/// <summary>Questions users can choose for "Forgot password?". They can also type their own.</summary>
public static class SecurityQuestions
{
    public static readonly string[] Presets =
    [
        "What is the name of your first pet?",
        "What is your mother's maiden name?",
        "What city were you born in?",
        "What was the name of your elementary school?",
        "What is your favorite food?",
        "What was the model of your first computer or phone?",
        "What is the name of your best friend in high school?",
    ];

    /// <summary>Answers are compared without caring about upper/lower case or extra spaces.</summary>
    public static string Normalize(string answer) =>
        string.Join(' ', answer.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));

    /// <summary>Returns an error message, or null when the question and answer are acceptable.</summary>
    public static string? Validate(string? question, string? answer)
    {
        if (string.IsNullOrWhiteSpace(question)) return "Please choose a security question.";
        if (question.Trim().Length < 8) return "Please write a longer security question.";
        if (string.IsNullOrWhiteSpace(answer) || Normalize(answer).Length < 2) return "Please enter an answer of at least 2 characters.";
        return null;
    }
}

/// <summary>Shared password rules.</summary>
public static class PasswordRules
{
    public const int MinLength = 6;

    public static string? Validate(string? password, string? confirm = null)
    {
        if (string.IsNullOrEmpty(password) || password.Length < MinLength) return $"Password must be at least {MinLength} characters.";
        if (confirm != null && password != confirm) return "The passwords do not match.";
        return null;
    }
}

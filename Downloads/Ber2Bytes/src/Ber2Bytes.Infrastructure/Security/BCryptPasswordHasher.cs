namespace Ber2Bytes.Infrastructure.Security;

/// <summary>Hashes passwords and security answers with BCrypt (slow, salted, safe to store).</summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string secret) => BCrypt.Net.BCrypt.HashPassword(secret);

    public bool Verify(string secret, string hash)
    {
        try { return BCrypt.Net.BCrypt.Verify(secret, hash); }
        catch { return false; } // malformed hash
    }
}

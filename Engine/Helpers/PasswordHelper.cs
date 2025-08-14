using Microsoft.AspNetCore.Identity;

namespace Engine.Helpers;
public static class PasswordHelper
{
    private static readonly PasswordHasher<object> hasher = new();

    public static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("A senha não pode ser nula ou vazia.", nameof(password));
        return hasher.HashPassword(null, password);
    }

    public static bool VerifyPassword(string hashedPassword, string password)
    {
        if (string.IsNullOrEmpty(hashedPassword))
            throw new ArgumentException("A senha hash não pode ser nula ou vazia.", nameof(hashedPassword));

        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("A senha não pode ser nula ou vazia.", nameof(password));

        var result = hasher.VerifyHashedPassword(null, hashedPassword, password);

        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}

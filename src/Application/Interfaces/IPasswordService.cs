namespace Application.Interfaces;

public interface IPasswordService
{
    /// <summary>
    /// Hashes a password using BCrypt with a secure salt
    /// </summary>
    /// <param name="password">The plain text password to hash</param>
    /// <returns>The hashed password</returns>
    string HashPassword(string password);
    
    /// <summary>
    /// Verifies a password against its hash
    /// </summary>
    /// <param name="password">The plain text password to verify</param>
    /// <param name="hash">The stored password hash</param>
    /// <returns>True if the password matches the hash, false otherwise</returns>
    bool VerifyPassword(string password, string hash);
    
    /// <summary>
    /// Generates a secure random password
    /// </summary>
    /// <param name="length">The length of the password to generate (default: 12)</param>
    /// <param name="includeSpecialChars">Whether to include special characters (default: true)</param>
    /// <returns>A randomly generated password</returns>
    string GenerateRandomPassword(int length = 12, bool includeSpecialChars = true);
}

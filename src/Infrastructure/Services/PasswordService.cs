using Application.Interfaces;
using BCrypt.Net;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services;

public class PasswordService : IPasswordService
{
    private const int WorkFactor = 12; // BCrypt work factor for hashing strength
    
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));
            
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }
    
    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;
            
        if (string.IsNullOrWhiteSpace(hash))
            return false;
            
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            // If verification fails due to invalid hash format, return false
            return false;
        }
    }
    
    public string GenerateRandomPassword(int length = 12, bool includeSpecialChars = true)
    {
        if (length < 4)
            throw new ArgumentException("Password length must be at least 4 characters", nameof(length));
            
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
        
        var characterSet = lowercase + uppercase + digits;
        if (includeSpecialChars)
            characterSet += specialChars;
            
        var password = new StringBuilder();
        
        // Ensure at least one character from each required set
        password.Append(GetRandomCharacter(lowercase));
        password.Append(GetRandomCharacter(uppercase));
        password.Append(GetRandomCharacter(digits));
        
        if (includeSpecialChars)
            password.Append(GetRandomCharacter(specialChars));
            
        // Fill the rest with random characters from the full set
        var remainingLength = length - password.Length;
        for (int i = 0; i < remainingLength; i++)
        {
            password.Append(GetRandomCharacter(characterSet));
        }
        
        // Shuffle the password to avoid predictable patterns
        return ShuffleString(password.ToString());
    }
    
    private static char GetRandomCharacter(string characterSet)
    {
        using var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[4];
        rng.GetBytes(randomBytes);
        var randomValue = BitConverter.ToUInt32(randomBytes, 0);
        return characterSet[(int)(randomValue % characterSet.Length)];
    }
    
    private static string ShuffleString(string input)
    {
        var array = input.ToCharArray();
        using var rng = RandomNumberGenerator.Create();
        
        for (int i = array.Length - 1; i > 0; i--)
        {
            var randomBytes = new byte[4];
            rng.GetBytes(randomBytes);
            var randomIndex = (int)(BitConverter.ToUInt32(randomBytes, 0) % (i + 1));
            
            (array[i], array[randomIndex]) = (array[randomIndex], array[i]);
        }
        
        return new string(array);
    }
}

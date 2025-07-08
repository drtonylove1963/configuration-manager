using System;
using BCrypt.Net;

class Program
{
    static void Main()
    {
        string password = "password123";
        string hash = BCrypt.Net.BCrypt.HashPassword(password, 11);
        
        Console.WriteLine($"Password: {password}");
        Console.WriteLine($"BCrypt Hash: {hash}");
        Console.WriteLine();
        Console.WriteLine("SQL to create test user:");
        Console.WriteLine($"INSERT INTO Users (Id, Username, Email, PasswordHash, FirstName, LastName, IsActive, IsEmailConfirmed, CreatedAt, CreatedBy, IsDeleted)");
        Console.WriteLine($"VALUES (NEWID(), 'testuser', 'testuser@configmanager.local', '{hash}', 'Test', 'User', 1, 1, GETUTCDATE(), 'system', 0);");
    }
}

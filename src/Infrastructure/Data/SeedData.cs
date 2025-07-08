using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(ConfigurationDbContext context, ILogger logger)
    {
        try
        {
            // Test connection first
            logger.LogInformation("Testing database connection...");
            await context.Database.CanConnectAsync();
            logger.LogInformation("Database connection successful");

            // Check if database exists and apply migrations if needed
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                logger.LogInformation($"Applying {pendingMigrations.Count()} pending migrations...");
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully");
            }
            else
            {
                // If no migrations are pending, ensure database is created
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Database ensured to exist");
            }

            // Seed Applications
            if (!await context.Applications.AnyAsync())
            {
                var applications = new[]
                {
                    new Domain.Entities.Application("Default Application", "Default application for configuration management", "system"),
                    new Domain.Entities.Application("Sample App", "Sample application for demonstration", "system"),
                    new Domain.Entities.Application("Test Application", "Application used for testing purposes", "system")
                };

                await context.Applications.AddRangeAsync(applications);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded {Count} applications", applications.Length);
            }

            // Seed Environments
            if (!await context.Environments.AnyAsync())
            {
                var environments = new[]
                {
                    new Domain.Entities.Environment("Development", "Development environment for testing", "system", 1),
                    new Domain.Entities.Environment("Staging", "Staging environment for pre-production testing", "system", 2),
                    new Domain.Entities.Environment("Production", "Production environment", "system", 3),
                    new Domain.Entities.Environment("Testing", "Testing environment for QA", "system", 4)
                };

                await context.Environments.AddRangeAsync(environments);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded {Count} environments", environments.Length);
            }

            // Seed Configuration Groups
            if (!await context.ConfigurationGroups.AnyAsync())
            {
                var groups = new[]
                {
                    new ConfigurationGroup("Database", "Database connection and settings", "system", null, 1),
                    new ConfigurationGroup("API", "API configuration settings", "system", null, 2),
                    new ConfigurationGroup("Security", "Security and authentication settings", "system", null, 3),
                    new ConfigurationGroup("Logging", "Logging configuration", "system", null, 4),
                    new ConfigurationGroup("Cache", "Caching configuration", "system", null, 5)
                };

                await context.ConfigurationGroups.AddRangeAsync(groups);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded {Count} configuration groups", groups.Length);

                // Add sub-groups
                var databaseGroup = groups[0];
                var subGroups = new[]
                {
                    new ConfigurationGroup("ConnectionStrings", "Database connection strings", "system", databaseGroup.Id, 1),
                    new ConfigurationGroup("Timeouts", "Database timeout settings", "system", databaseGroup.Id, 2)
                };

                await context.ConfigurationGroups.AddRangeAsync(subGroups);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded {Count} configuration sub-groups", subGroups.Length);
            }

            // Seed Sample Configurations
            if (!await context.Configurations.AnyAsync())
            {
                var applications = await context.Applications.ToListAsync();
                var environments = await context.Environments.ToListAsync();
                var groups = await context.ConfigurationGroups.ToListAsync();

                var defaultApplication = applications.First(a => a.Name == "Default Application");
                var databaseGroup = groups.First(g => g.Name == "Database");
                var apiGroup = groups.First(g => g.Name == "API");
                var securityGroup = groups.First(g => g.Name == "Security");
                var connectionStringsGroup = groups.First(g => g.Name == "ConnectionStrings");

                var sampleConfigurations = new List<Configuration>();

                foreach (var env in environments)
                {
                    // Database configurations
                    sampleConfigurations.AddRange(new[]
                    {
                        new Configuration(
                            "Database.ConnectionString",
                            GetConnectionStringForEnvironment(env.Name),
                            ConfigurationValueType.String,
                            "Main database connection string",
                            defaultApplication.Id,
                            env.Id,
                            "system",
                            connectionStringsGroup.Id,
                            true, // encrypted
                            true  // required
                        ),
                        new Configuration(
                            "Database.CommandTimeout",
                            "30",
                            ConfigurationValueType.Integer,
                            "Database command timeout in seconds",
                            defaultApplication.Id,
                            env.Id,
                            "system",
                            databaseGroup.Id,
                            false,
                            true
                        ),

                        // API configurations
                        new Configuration(
                            "API.BaseUrl",
                            GetApiUrlForEnvironment(env.Name),
                            ConfigurationValueType.String,
                            "Base URL for the API",
                            defaultApplication.Id,
                            env.Id,
                            "system",
                            apiGroup.Id,
                            false,
                            true
                        ),
                        new Configuration(
                            "API.RequestTimeout",
                            "60",
                            ConfigurationValueType.Integer,
                            "API request timeout in seconds",
                            defaultApplication.Id,
                            env.Id,
                            "system",
                            apiGroup.Id,
                            false,
                            true
                        ),

                        // Security configurations
                        new Configuration(
                            "Security.JwtSecret",
                            GenerateJwtSecret(),
                            ConfigurationValueType.String,
                            "JWT signing secret",
                            defaultApplication.Id,
                            env.Id,
                            "system",
                            securityGroup.Id,
                            true, // encrypted
                            true  // required
                        ),
                        new Configuration(
                            "Security.TokenExpiryMinutes",
                            env.Name == "Production" ? "60" : "480",
                            ConfigurationValueType.Integer,
                            "JWT token expiry time in minutes",
                            defaultApplication.Id,
                            env.Id,
                            "system",
                            securityGroup.Id,
                            false,
                            true
                        ),

                        // Feature flags
                        new Configuration(
                            "Features.EnableAuditLogging",
                            "true",
                            ConfigurationValueType.Boolean,
                            "Enable audit logging feature",
                            defaultApplication.Id,
                            env.Id,
                            "system",
                            null,
                            false,
                            false
                        ),
                        new Configuration(
                            "Features.EnableCaching",
                            env.Name == "Production" ? "true" : "false",
                            ConfigurationValueType.Boolean,
                            "Enable caching feature",
                            defaultApplication.Id,
                            env.Id,
                            "system",
                            null,
                            false,
                            false
                        )
                    });
                }

                await context.Configurations.AddRangeAsync(sampleConfigurations);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded {Count} sample configurations", sampleConfigurations.Count);
            }

            // Seed Users
            if (!await context.Users.AnyAsync())
            {
                var users = new[]
                {
                    new User(
                        "testuser",
                        "testuser@configmanager.local",
                        BCrypt.Net.BCrypt.HashPassword("password123", 11),
                        "Test",
                        "User",
                        "system"
                    ),
                    new User(
                        "admin",
                        "admin@configmanager.local",
                        BCrypt.Net.BCrypt.HashPassword("password123", 11),
                        "Admin",
                        "User",
                        "system"
                    )
                };

                await context.Users.AddRangeAsync(users);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded {Count} test users", users.Length);
            }

            logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private static string GetConnectionStringForEnvironment(string environmentName)
    {
        return environmentName switch
        {
            "Development" => "Server=localhost,1433;Database=ConfigurationManager_Dev;User Id=sa;Password=ConfigManager123!;TrustServerCertificate=true;",
            "Staging" => "Server=localhost,1433;Database=ConfigurationManager_Staging;User Id=sa;Password=ConfigManager123!;TrustServerCertificate=true;",
            "Production" => "Server=localhost,1433;Database=ConfigurationManager_Prod;User Id=sa;Password=ConfigManager123!;TrustServerCertificate=true;",
            "Testing" => "Server=localhost,1433;Database=ConfigurationManager_Test;User Id=sa;Password=ConfigManager123!;TrustServerCertificate=true;",
            _ => "Server=localhost,1433;Database=ConfigurationManager;User Id=sa;Password=ConfigManager123!;TrustServerCertificate=true;"
        };
    }

    private static string GetApiUrlForEnvironment(string environmentName)
    {
        return environmentName switch
        {
            "Development" => "https://localhost:7001",
            "Staging" => "https://staging-api.configmanager.com",
            "Production" => "https://api.configmanager.com",
            "Testing" => "https://test-api.configmanager.com",
            _ => "https://localhost:7001"
        };
    }

    private static string GenerateJwtSecret()
    {
        // Generate a random JWT secret for demo purposes
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, 64)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}

using Application.Interfaces;
using Application.Settings;
using Domain.Repositories;
using Infrastructure.Data;
using Infrastructure.Messaging;
using Infrastructure.MongoDB;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Entity Framework
        services.AddDbContext<ConfigurationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Add MongoDB
        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDB"));
        services.AddSingleton<MongoDbContext>();

        // Add RabbitMQ
        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMQ"));
        services.AddSingleton<IMessageBusService, RabbitMqService>();

        // Add JWT Configuration
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // Add Repositories
        services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
        services.AddScoped<IEnvironmentRepository, EnvironmentRepository>();
        services.AddScoped<IConfigurationGroupRepository, ConfigurationGroupRepository>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Add Infrastructure Services
        services.AddScoped<IConfigurationCacheService, ConfigurationCacheService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }

    public static IServiceCollection AddInfrastructureHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        // Health checks temporarily disabled due to SQL Server SSL issues
        services.AddHealthChecks();

        return services;
    }
}

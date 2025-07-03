using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register AutoMapper
        services.AddAutoMapper(typeof(ConfigurationMappingProfile));

        // Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // Register Application Services
        services.AddScoped<IConfigurationService, ConfigurationService>();
        services.AddScoped<IEnvironmentService, EnvironmentService>();
        services.AddScoped<IConfigurationGroupService, ConfigurationGroupService>();

        return services;
    }
}

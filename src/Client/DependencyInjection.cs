using Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Client;

public static class DependencyInjection
{
    public static IServiceCollection AddClient(this IServiceCollection services, string apiBaseUrl)
    {
        // Add authentication services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<CustomAuthenticationStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthenticationStateProvider>());
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddTransient<JwtAuthenticationHandler>();

        // Add authentication HTTP client
        services.AddHttpClient<IAuthenticationApiClient, AuthenticationApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
        });

        // Add HTTP clients with JWT authentication
        services.AddHttpClient<IConfigurationApiClient, ConfigurationApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
        }).AddHttpMessageHandler<JwtAuthenticationHandler>();

        services.AddHttpClient<IEnvironmentApiClient, EnvironmentApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
        }).AddHttpMessageHandler<JwtAuthenticationHandler>();

        services.AddHttpClient<IConfigurationGroupApiClient, ConfigurationGroupApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
        }).AddHttpMessageHandler<JwtAuthenticationHandler>();

        // AuditApiClient temporarily removed for testing
        // services.AddHttpClient<IAuditApiClient, AuditApiClient>(client =>
        // {
        //     client.BaseAddress = new Uri(apiBaseUrl);
        // }).AddHttpMessageHandler<JwtAuthenticationHandler>();

        return services;
    }
}

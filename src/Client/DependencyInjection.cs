using Client.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Client;

public static class DependencyInjection
{
    public static IServiceCollection AddClient(this IServiceCollection services, string apiBaseUrl)
    {
        // Add HTTP clients
        services.AddHttpClient<IConfigurationApiClient, ConfigurationApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
        });

        services.AddHttpClient<IEnvironmentApiClient, EnvironmentApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
        });

        services.AddHttpClient<IConfigurationGroupApiClient, ConfigurationGroupApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
        });

        // AuditApiClient temporarily removed for testing
        // services.AddHttpClient<IAuditApiClient, AuditApiClient>(client =>
        // {
        //     client.BaseAddress = new Uri(apiBaseUrl);
        // });

        return services;
    }
}

using Application.DTOs.Configuration;
using System.Text;
using System.Text.Json;

namespace Client.Services;

public interface IConfigurationApiClient
{
    Task<IEnumerable<ConfigurationDto>> GetAllAsync();
    Task<ConfigurationDto?> GetByIdAsync(Guid id);
    Task<ConfigurationDto?> GetByKeyAndEnvironmentAsync(string key, Guid environmentId);
    Task<IEnumerable<ConfigurationDto>> GetByEnvironmentAsync(Guid environmentId);
    Task<IEnumerable<ConfigurationDto>> GetByGroupAsync(Guid groupId);
    Task<IEnumerable<ConfigurationDto>> SearchAsync(ConfigurationSearchDto searchDto);
    Task<ConfigurationDto> CreateAsync(CreateConfigurationDto createDto);
    Task<ConfigurationDto> UpdateAsync(Guid id, UpdateConfigurationDto updateDto);
    Task DeleteAsync(Guid id);
    Task ActivateAsync(Guid id);
    Task DeactivateAsync(Guid id);
    Task<IEnumerable<ConfigurationHistoryDto>> GetHistoryAsync(Guid id);
    Task BulkUpdateAsync(BulkConfigurationUpdateDto bulkUpdateDto);
    Task<Dictionary<string, string>> GetEnvironmentKeyValuesAsync(Guid environmentId, bool activeOnly = true);
}

public class ConfigurationApiClient : IConfigurationApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ConfigurationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<IEnumerable<ConfigurationDto>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("api/v1/configurations");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<ConfigurationDto>>(json, _jsonOptions) ?? Enumerable.Empty<ConfigurationDto>();
    }

    public async Task<ConfigurationDto?> GetByIdAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"api/v1/configurations/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ConfigurationDto>(json, _jsonOptions);
    }

    public async Task<ConfigurationDto?> GetByKeyAndEnvironmentAsync(string key, Guid environmentId)
    {
        var response = await _httpClient.GetAsync($"api/v1/configurations/by-key?key={Uri.EscapeDataString(key)}&environmentId={environmentId}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ConfigurationDto>(json, _jsonOptions);
    }

    public async Task<IEnumerable<ConfigurationDto>> GetByEnvironmentAsync(Guid environmentId)
    {
        var response = await _httpClient.GetAsync($"api/v1/configurations/environment/{environmentId}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<ConfigurationDto>>(json, _jsonOptions) ?? Enumerable.Empty<ConfigurationDto>();
    }

    public async Task<IEnumerable<ConfigurationDto>> GetByGroupAsync(Guid groupId)
    {
        var response = await _httpClient.GetAsync($"api/v1/configurations/group/{groupId}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<ConfigurationDto>>(json, _jsonOptions) ?? Enumerable.Empty<ConfigurationDto>();
    }

    public async Task<IEnumerable<ConfigurationDto>> SearchAsync(ConfigurationSearchDto searchDto)
    {
        var json = JsonSerializer.Serialize(searchDto, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/v1/configurations/search", content);
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<ConfigurationDto>>(responseJson, _jsonOptions) ?? Enumerable.Empty<ConfigurationDto>();
    }

    public async Task<ConfigurationDto> CreateAsync(CreateConfigurationDto createDto)
    {
        var json = JsonSerializer.Serialize(createDto, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/v1/configurations", content);
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ConfigurationDto>(responseJson, _jsonOptions)!;
    }

    public async Task<ConfigurationDto> UpdateAsync(Guid id, UpdateConfigurationDto updateDto)
    {
        var json = JsonSerializer.Serialize(updateDto, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"api/v1/configurations/{id}", content);
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ConfigurationDto>(responseJson, _jsonOptions)!;
    }

    public async Task DeleteAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"api/v1/configurations/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task ActivateAsync(Guid id)
    {
        var response = await _httpClient.PostAsync($"api/v1/configurations/{id}/activate", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeactivateAsync(Guid id)
    {
        var response = await _httpClient.PostAsync($"api/v1/configurations/{id}/deactivate", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IEnumerable<ConfigurationHistoryDto>> GetHistoryAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"api/v1/configurations/{id}/history");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<ConfigurationHistoryDto>>(json, _jsonOptions) ?? Enumerable.Empty<ConfigurationHistoryDto>();
    }

    public async Task BulkUpdateAsync(BulkConfigurationUpdateDto bulkUpdateDto)
    {
        var json = JsonSerializer.Serialize(bulkUpdateDto, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/v1/configurations/bulk-update", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Dictionary<string, string>> GetEnvironmentKeyValuesAsync(Guid environmentId, bool activeOnly = true)
    {
        var response = await _httpClient.GetAsync($"api/v1/configurations/environment/{environmentId}/key-values?activeOnly={activeOnly}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json, _jsonOptions) ?? new Dictionary<string, string>();
    }
}

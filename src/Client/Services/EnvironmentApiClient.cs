using Application.DTOs.Environment;
using System.Text;
using System.Text.Json;

namespace Client.Services;

public interface IEnvironmentApiClient
{
    Task<IEnumerable<EnvironmentDto>> GetAllAsync();
    Task<IEnumerable<EnvironmentDto>> GetActiveAsync();
    Task<IEnumerable<EnvironmentSummaryDto>> GetSummariesAsync();
    Task<EnvironmentDto?> GetByIdAsync(Guid id);
    Task<EnvironmentDto?> GetByNameAsync(string name);
    Task<EnvironmentDto> CreateAsync(CreateEnvironmentDto createDto);
    Task<EnvironmentDto> UpdateAsync(Guid id, UpdateEnvironmentDto updateDto);
    Task DeleteAsync(Guid id);
    Task ActivateAsync(Guid id);
    Task DeactivateAsync(Guid id);
    Task<bool> ExistsAsync(string name);
}

public class EnvironmentApiClient : IEnvironmentApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public EnvironmentApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<IEnumerable<EnvironmentDto>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("api/v1/environments");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<EnvironmentDto>>(json, _jsonOptions) ?? Enumerable.Empty<EnvironmentDto>();
    }

    public async Task<IEnumerable<EnvironmentDto>> GetActiveAsync()
    {
        var response = await _httpClient.GetAsync("api/v1/environments/active");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<EnvironmentDto>>(json, _jsonOptions) ?? Enumerable.Empty<EnvironmentDto>();
    }

    public async Task<IEnumerable<EnvironmentSummaryDto>> GetSummariesAsync()
    {
        var response = await _httpClient.GetAsync("api/v1/environments/summaries");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<EnvironmentSummaryDto>>(json, _jsonOptions) ?? Enumerable.Empty<EnvironmentSummaryDto>();
    }

    public async Task<EnvironmentDto?> GetByIdAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"api/v1/environments/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<EnvironmentDto>(json, _jsonOptions);
    }

    public async Task<EnvironmentDto?> GetByNameAsync(string name)
    {
        var response = await _httpClient.GetAsync($"api/v1/environments/by-name/{Uri.EscapeDataString(name)}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<EnvironmentDto>(json, _jsonOptions);
    }

    public async Task<EnvironmentDto> CreateAsync(CreateEnvironmentDto createDto)
    {
        var json = JsonSerializer.Serialize(createDto, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/v1/environments", content);
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<EnvironmentDto>(responseJson, _jsonOptions)!;
    }

    public async Task<EnvironmentDto> UpdateAsync(Guid id, UpdateEnvironmentDto updateDto)
    {
        var json = JsonSerializer.Serialize(updateDto, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"api/v1/environments/{id}", content);
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<EnvironmentDto>(responseJson, _jsonOptions)!;
    }

    public async Task DeleteAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"api/v1/environments/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task ActivateAsync(Guid id)
    {
        var response = await _httpClient.PostAsync($"api/v1/environments/{id}/activate", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeactivateAsync(Guid id)
    {
        var response = await _httpClient.PostAsync($"api/v1/environments/{id}/deactivate", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> ExistsAsync(string name)
    {
        var response = await _httpClient.GetAsync($"api/v1/environments/exists/{Uri.EscapeDataString(name)}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<bool>(json, _jsonOptions);
    }
}

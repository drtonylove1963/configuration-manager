using Application.DTOs.ConfigurationGroup;
using System.Text;
using System.Text.Json;

namespace Client.Services;

public interface IConfigurationGroupApiClient
{
    Task<IEnumerable<ConfigurationGroupDto>> GetAllAsync();
    Task<IEnumerable<ConfigurationGroupDto>> GetRootGroupsAsync();
    Task<IEnumerable<ConfigurationGroupTreeDto>> GetGroupTreeAsync();
    Task<ConfigurationGroupDto?> GetByIdAsync(Guid id);
    Task<ConfigurationGroupDto?> GetByNameAsync(string name);
    Task<IEnumerable<ConfigurationGroupDto>> GetByParentAsync(Guid parentId);
    Task<ConfigurationGroupDto> CreateAsync(CreateConfigurationGroupDto createDto);
    Task<ConfigurationGroupDto> UpdateAsync(Guid id, UpdateConfigurationGroupDto updateDto);
    Task<ConfigurationGroupDto> MoveAsync(Guid id, MoveGroupDto moveDto);
    Task DeleteAsync(Guid id);
    Task ActivateAsync(Guid id);
    Task DeactivateAsync(Guid id);
    Task<bool> ExistsAsync(string name);
}

public class ConfigurationGroupApiClient : IConfigurationGroupApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ConfigurationGroupApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<IEnumerable<ConfigurationGroupDto>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("api/v1/configurationgroups");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<ConfigurationGroupDto>>(json, _jsonOptions) ?? Enumerable.Empty<ConfigurationGroupDto>();
    }

    public async Task<IEnumerable<ConfigurationGroupDto>> GetRootGroupsAsync()
    {
        var response = await _httpClient.GetAsync("api/v1/configurationgroups/root");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<ConfigurationGroupDto>>(json, _jsonOptions) ?? Enumerable.Empty<ConfigurationGroupDto>();
    }

    public async Task<IEnumerable<ConfigurationGroupTreeDto>> GetGroupTreeAsync()
    {
        var response = await _httpClient.GetAsync("api/v1/configurationgroups/tree");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<ConfigurationGroupTreeDto>>(json, _jsonOptions) ?? Enumerable.Empty<ConfigurationGroupTreeDto>();
    }

    public async Task<ConfigurationGroupDto?> GetByIdAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"api/v1/configurationgroups/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ConfigurationGroupDto>(json, _jsonOptions);
    }

    public async Task<ConfigurationGroupDto?> GetByNameAsync(string name)
    {
        var response = await _httpClient.GetAsync($"api/v1/configurationgroups/by-name/{Uri.EscapeDataString(name)}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ConfigurationGroupDto>(json, _jsonOptions);
    }

    public async Task<IEnumerable<ConfigurationGroupDto>> GetByParentAsync(Guid parentId)
    {
        var response = await _httpClient.GetAsync($"api/v1/configurationgroups/parent/{parentId}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<ConfigurationGroupDto>>(json, _jsonOptions) ?? Enumerable.Empty<ConfigurationGroupDto>();
    }

    public async Task<ConfigurationGroupDto> CreateAsync(CreateConfigurationGroupDto createDto)
    {
        var json = JsonSerializer.Serialize(createDto, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/v1/configurationgroups", content);
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ConfigurationGroupDto>(responseJson, _jsonOptions)!;
    }

    public async Task<ConfigurationGroupDto> UpdateAsync(Guid id, UpdateConfigurationGroupDto updateDto)
    {
        var json = JsonSerializer.Serialize(updateDto, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"api/v1/configurationgroups/{id}", content);
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ConfigurationGroupDto>(responseJson, _jsonOptions)!;
    }

    public async Task<ConfigurationGroupDto> MoveAsync(Guid id, MoveGroupDto moveDto)
    {
        var json = JsonSerializer.Serialize(moveDto, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"api/v1/configurationgroups/{id}/move", content);
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ConfigurationGroupDto>(responseJson, _jsonOptions)!;
    }

    public async Task DeleteAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"api/v1/configurationgroups/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task ActivateAsync(Guid id)
    {
        var response = await _httpClient.PostAsync($"api/v1/configurationgroups/{id}/activate", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeactivateAsync(Guid id)
    {
        var response = await _httpClient.PostAsync($"api/v1/configurationgroups/{id}/deactivate", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> ExistsAsync(string name)
    {
        var response = await _httpClient.GetAsync($"api/v1/configurationgroups/exists/{Uri.EscapeDataString(name)}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<bool>(json, _jsonOptions);
    }
}

using Application.DTOs.Configuration;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfigurationRepository _configurationRepository;
    private readonly IEnvironmentRepository _environmentRepository;
    private readonly IConfigurationGroupRepository _groupRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ConfigurationService> _logger;

    public ConfigurationService(
        IConfigurationRepository configurationRepository,
        IEnvironmentRepository environmentRepository,
        IConfigurationGroupRepository groupRepository,
        IMapper mapper,
        ILogger<ConfigurationService> logger)
    {
        _configurationRepository = configurationRepository;
        _environmentRepository = environmentRepository;
        _groupRepository = groupRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ConfigurationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var configuration = await _configurationRepository.GetByIdAsync(id, cancellationToken);
        return configuration != null ? _mapper.Map<ConfigurationDto>(configuration) : null;
    }

    public async Task<ConfigurationDto?> GetByKeyAndEnvironmentAsync(string key, Guid environmentId, CancellationToken cancellationToken = default)
    {
        var configuration = await _configurationRepository.GetByKeyAndEnvironmentAsync(key, environmentId, cancellationToken);
        return configuration != null ? _mapper.Map<ConfigurationDto>(configuration) : null;
    }

    public async Task<IEnumerable<ConfigurationDto>> GetByEnvironmentAsync(Guid environmentId, CancellationToken cancellationToken = default)
    {
        var configurations = await _configurationRepository.GetByEnvironmentAsync(environmentId, cancellationToken);
        return _mapper.Map<IEnumerable<ConfigurationDto>>(configurations);
    }

    public async Task<IEnumerable<ConfigurationDto>> GetByGroupAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        var configurations = await _configurationRepository.GetByGroupAsync(groupId, cancellationToken);
        return _mapper.Map<IEnumerable<ConfigurationDto>>(configurations);
    }

    public async Task<IEnumerable<ConfigurationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var configurations = await _configurationRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<ConfigurationDto>>(configurations);
    }

    public async Task<IEnumerable<ConfigurationDto>> SearchAsync(ConfigurationSearchDto searchDto, CancellationToken cancellationToken = default)
    {
        var configurations = await _configurationRepository.SearchAsync(
            searchDto.SearchTerm ?? string.Empty,
            searchDto.EnvironmentId,
            searchDto.GroupId,
            cancellationToken);
        
        var result = _mapper.Map<IEnumerable<ConfigurationDto>>(configurations);
        
        // Apply additional filters
        if (searchDto.IsActive.HasValue)
        {
            result = result.Where(c => c.IsActive == searchDto.IsActive.Value);
        }
        
        if (searchDto.ValueType.HasValue)
        {
            result = result.Where(c => c.ValueType == searchDto.ValueType.Value);
        }
        
        return result;
    }

    public async Task<ConfigurationDto> CreateAsync(CreateConfigurationDto createDto, string createdBy, CancellationToken cancellationToken = default)
    {
        // Validate environment exists
        var environment = await _environmentRepository.GetByIdAsync(createDto.EnvironmentId, cancellationToken);
        if (environment == null)
        {
            throw new EnvironmentNotFoundException(createDto.EnvironmentId);
        }

        // Validate group exists if specified
        if (createDto.GroupId.HasValue)
        {
            var group = await _groupRepository.GetByIdAsync(createDto.GroupId.Value, cancellationToken);
            if (group == null)
            {
                throw new ConfigurationGroupNotFoundException(createDto.GroupId.Value);
            }
        }

        // Check if configuration already exists
        var exists = await _configurationRepository.ExistsAsync(createDto.Key, createDto.EnvironmentId, cancellationToken);
        if (exists)
        {
            throw new ConfigurationAlreadyExistsException(createDto.Key, environment.Name);
        }

        var configuration = new Configuration(
            createDto.Key,
            createDto.Value,
            createDto.ValueType,
            createDto.Description,
            createDto.ApplicationId,
            createDto.EnvironmentId,
            createdBy,
            createDto.GroupId,
            createDto.IsEncrypted,
            createDto.IsRequired,
            createDto.DefaultValue);

        await _configurationRepository.AddAsync(configuration, cancellationToken);

        _logger.LogInformation("Configuration {Key} created in environment {Environment} by {User}",
            createDto.Key, environment.Name, createdBy);

        return _mapper.Map<ConfigurationDto>(configuration);
    }

    public async Task<ConfigurationDto> UpdateAsync(Guid id, UpdateConfigurationDto updateDto, string updatedBy, CancellationToken cancellationToken = default)
    {
        var configuration = await _configurationRepository.GetByIdAsync(id, cancellationToken);
        if (configuration == null)
        {
            throw new ConfigurationNotFoundException(id);
        }

        // Validate group exists if specified
        if (updateDto.GroupId.HasValue)
        {
            var group = await _groupRepository.GetByIdAsync(updateDto.GroupId.Value, cancellationToken);
            if (group == null)
            {
                throw new ConfigurationGroupNotFoundException(updateDto.GroupId.Value);
            }
        }

        configuration.UpdateValue(updateDto.Value, updateDto.ValueType, updatedBy, updateDto.ChangeReason);
        configuration.UpdateDetails(updateDto.Description, updatedBy, updateDto.GroupId, updateDto.IsRequired, updateDto.DefaultValue);

        await _configurationRepository.UpdateAsync(configuration, cancellationToken);

        _logger.LogInformation("Configuration {Key} updated by {User}. Reason: {Reason}",
            configuration.Key.Value, updatedBy, updateDto.ChangeReason ?? "No reason provided");

        return _mapper.Map<ConfigurationDto>(configuration);
    }

    public async Task DeleteAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default)
    {
        var configuration = await _configurationRepository.GetByIdAsync(id, cancellationToken);
        if (configuration == null)
        {
            throw new ConfigurationNotFoundException(id);
        }

        configuration.Delete(deletedBy);
        await _configurationRepository.UpdateAsync(configuration, cancellationToken);

        _logger.LogInformation("Configuration {Key} deleted by {User}",
            configuration.Key.Value, deletedBy);
    }

    public async Task<IEnumerable<ConfigurationHistoryDto>> GetHistoryAsync(Guid configurationId, CancellationToken cancellationToken = default)
    {
        var history = await _configurationRepository.GetHistoryAsync(configurationId, cancellationToken);
        return _mapper.Map<IEnumerable<ConfigurationHistoryDto>>(history);
    }

    public async Task ActivateAsync(Guid id, string updatedBy, CancellationToken cancellationToken = default)
    {
        var configuration = await _configurationRepository.GetByIdAsync(id, cancellationToken);
        if (configuration == null)
        {
            throw new ConfigurationNotFoundException(id);
        }

        configuration.Activate(updatedBy);
        await _configurationRepository.UpdateAsync(configuration, cancellationToken);

        _logger.LogInformation("Configuration {Key} activated by {User}",
            configuration.Key.Value, updatedBy);
    }

    public async Task DeactivateAsync(Guid id, string updatedBy, CancellationToken cancellationToken = default)
    {
        var configuration = await _configurationRepository.GetByIdAsync(id, cancellationToken);
        if (configuration == null)
        {
            throw new ConfigurationNotFoundException(id);
        }

        configuration.Deactivate(updatedBy);
        await _configurationRepository.UpdateAsync(configuration, cancellationToken);

        _logger.LogInformation("Configuration {Key} deactivated by {User}",
            configuration.Key.Value, updatedBy);
    }

    public async Task BulkUpdateAsync(BulkConfigurationUpdateDto bulkUpdateDto, string updatedBy, CancellationToken cancellationToken = default)
    {
        foreach (var configId in bulkUpdateDto.ConfigurationIds)
        {
            var configuration = await _configurationRepository.GetByIdAsync(configId, cancellationToken);
            if (configuration == null) continue;

            if (bulkUpdateDto.NewGroupId != null)
            {
                var groupId = string.IsNullOrEmpty(bulkUpdateDto.NewGroupId) ? (Guid?)null : Guid.Parse(bulkUpdateDto.NewGroupId);
                configuration.UpdateDetails(configuration.Description, updatedBy, groupId);
            }

            if (bulkUpdateDto.IsActive.HasValue)
            {
                if (bulkUpdateDto.IsActive.Value)
                    configuration.Activate(updatedBy);
                else
                    configuration.Deactivate(updatedBy);
            }

            await _configurationRepository.UpdateAsync(configuration, cancellationToken);
        }

        _logger.LogInformation("Bulk update performed on {Count} configurations by {User}. Reason: {Reason}",
            bulkUpdateDto.ConfigurationIds.Length, updatedBy, bulkUpdateDto.ChangeReason ?? "No reason provided");
    }

    public async Task<bool> ExistsAsync(string key, Guid environmentId, CancellationToken cancellationToken = default)
    {
        return await _configurationRepository.ExistsAsync(key, environmentId, cancellationToken);
    }

    public async Task<Dictionary<string, string>> GetEnvironmentConfigurationsAsync(Guid environmentId, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var configurations = await _configurationRepository.GetByEnvironmentAsync(environmentId, cancellationToken);
        
        if (activeOnly)
        {
            configurations = configurations.Where(c => c.IsActive && !c.IsDeleted);
        }

        return configurations.ToDictionary(c => c.Key.Value, c => c.Value.Value);
    }
}

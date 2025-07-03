using Application.DTOs.ConfigurationGroup;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ConfigurationGroupService : IConfigurationGroupService
{
    private readonly IConfigurationGroupRepository _groupRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ConfigurationGroupService> _logger;

    public ConfigurationGroupService(
        IConfigurationGroupRepository groupRepository,
        IMapper mapper,
        ILogger<ConfigurationGroupService> logger)
    {
        _groupRepository = groupRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ConfigurationGroupDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.GetByIdAsync(id, cancellationToken);
        return group != null ? _mapper.Map<ConfigurationGroupDto>(group) : null;
    }

    public async Task<ConfigurationGroupDto?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.GetByNameAsync(name, cancellationToken);
        return group != null ? _mapper.Map<ConfigurationGroupDto>(group) : null;
    }

    public async Task<IEnumerable<ConfigurationGroupDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var groups = await _groupRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<ConfigurationGroupDto>>(groups);
    }

    public async Task<IEnumerable<ConfigurationGroupDto>> GetByParentAsync(Guid? parentId, CancellationToken cancellationToken = default)
    {
        var groups = await _groupRepository.GetByParentAsync(parentId, cancellationToken);
        return _mapper.Map<IEnumerable<ConfigurationGroupDto>>(groups);
    }

    public async Task<IEnumerable<ConfigurationGroupDto>> GetRootGroupsAsync(CancellationToken cancellationToken = default)
    {
        var groups = await _groupRepository.GetRootGroupsAsync(cancellationToken);
        return _mapper.Map<IEnumerable<ConfigurationGroupDto>>(groups);
    }

    public async Task<IEnumerable<ConfigurationGroupTreeDto>> GetGroupTreeAsync(CancellationToken cancellationToken = default)
    {
        var rootGroups = await _groupRepository.GetRootGroupsAsync(cancellationToken);
        return _mapper.Map<IEnumerable<ConfigurationGroupTreeDto>>(rootGroups);
    }

    public async Task<ConfigurationGroupDto> CreateAsync(CreateConfigurationGroupDto createDto, string createdBy, CancellationToken cancellationToken = default)
    {
        // Check if group already exists
        var exists = await _groupRepository.ExistsAsync(createDto.Name, cancellationToken);
        if (exists)
        {
            throw new ConfigurationGroupAlreadyExistsException(createDto.Name);
        }

        // Validate parent group exists if specified
        if (createDto.ParentGroupId.HasValue)
        {
            var parentGroup = await _groupRepository.GetByIdAsync(createDto.ParentGroupId.Value, cancellationToken);
            if (parentGroup == null)
            {
                throw new ConfigurationGroupNotFoundException(createDto.ParentGroupId.Value);
            }
        }

        var group = new ConfigurationGroup(
            createDto.Name,
            createDto.Description,
            createdBy,
            createDto.ParentGroupId,
            createDto.SortOrder);

        await _groupRepository.AddAsync(group, cancellationToken);

        _logger.LogInformation("Configuration group {Name} created by {User}", createDto.Name, createdBy);

        return _mapper.Map<ConfigurationGroupDto>(group);
    }

    public async Task<ConfigurationGroupDto> UpdateAsync(Guid id, UpdateConfigurationGroupDto updateDto, string updatedBy, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.GetByIdAsync(id, cancellationToken);
        if (group == null)
        {
            throw new ConfigurationGroupNotFoundException(id);
        }

        // Check if new name conflicts with existing group (if name is being changed)
        if (group.Name != updateDto.Name)
        {
            var exists = await _groupRepository.ExistsAsync(updateDto.Name, cancellationToken);
            if (exists)
            {
                throw new ConfigurationGroupAlreadyExistsException(updateDto.Name);
            }
        }

        group.UpdateDetails(updateDto.Name, updateDto.Description, updatedBy, updateDto.SortOrder);
        await _groupRepository.UpdateAsync(group, cancellationToken);

        _logger.LogInformation("Configuration group {Name} updated by {User}", updateDto.Name, updatedBy);

        return _mapper.Map<ConfigurationGroupDto>(group);
    }

    public async Task DeleteAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.GetByIdAsync(id, cancellationToken);
        if (group == null)
        {
            throw new ConfigurationGroupNotFoundException(id);
        }

        group.Delete(deletedBy);
        await _groupRepository.UpdateAsync(group, cancellationToken);

        _logger.LogInformation("Configuration group {Name} deleted by {User}", group.Name, deletedBy);
    }

    public async Task<ConfigurationGroupDto> MoveGroupAsync(Guid id, MoveGroupDto moveDto, string updatedBy, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.GetByIdAsync(id, cancellationToken);
        if (group == null)
        {
            throw new ConfigurationGroupNotFoundException(id);
        }

        // Validate new parent group exists if specified
        if (moveDto.NewParentGroupId.HasValue)
        {
            // Check for circular reference
            if (await WouldCreateCircularReference(id, moveDto.NewParentGroupId.Value, cancellationToken))
            {
                throw new CircularReferenceException(group.Name);
            }

            var newParentGroup = await _groupRepository.GetByIdAsync(moveDto.NewParentGroupId.Value, cancellationToken);
            if (newParentGroup == null)
            {
                throw new ConfigurationGroupNotFoundException(moveDto.NewParentGroupId.Value);
            }
        }

        group.ChangeParent(moveDto.NewParentGroupId, updatedBy);
        await _groupRepository.UpdateAsync(group, cancellationToken);

        _logger.LogInformation("Configuration group {Name} moved by {User}", group.Name, updatedBy);

        return _mapper.Map<ConfigurationGroupDto>(group);
    }

    public async Task ActivateAsync(Guid id, string updatedBy, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.GetByIdAsync(id, cancellationToken);
        if (group == null)
        {
            throw new ConfigurationGroupNotFoundException(id);
        }

        group.Activate(updatedBy);
        await _groupRepository.UpdateAsync(group, cancellationToken);

        _logger.LogInformation("Configuration group {Name} activated by {User}", group.Name, updatedBy);
    }

    public async Task DeactivateAsync(Guid id, string updatedBy, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.GetByIdAsync(id, cancellationToken);
        if (group == null)
        {
            throw new ConfigurationGroupNotFoundException(id);
        }

        group.Deactivate(updatedBy);
        await _groupRepository.UpdateAsync(group, cancellationToken);

        _logger.LogInformation("Configuration group {Name} deactivated by {User}", group.Name, updatedBy);
    }

    public async Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _groupRepository.ExistsAsync(name, cancellationToken);
    }

    private async Task<bool> WouldCreateCircularReference(Guid groupId, Guid newParentId, CancellationToken cancellationToken)
    {
        var currentParentId = newParentId;
        
        while (currentParentId != Guid.Empty)
        {
            if (currentParentId == groupId)
            {
                return true; // Circular reference detected
            }

            var parentGroup = await _groupRepository.GetByIdAsync(currentParentId, cancellationToken);
            if (parentGroup?.ParentGroupId == null)
            {
                break;
            }

            currentParentId = parentGroup.ParentGroupId.Value;
        }

        return false;
    }
}

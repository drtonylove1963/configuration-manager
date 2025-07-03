using Application.DTOs.Environment;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class EnvironmentService : IEnvironmentService
{
    private readonly IEnvironmentRepository _environmentRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<EnvironmentService> _logger;

    public EnvironmentService(
        IEnvironmentRepository environmentRepository,
        IMapper mapper,
        ILogger<EnvironmentService> logger)
    {
        _environmentRepository = environmentRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<EnvironmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var environment = await _environmentRepository.GetByIdAsync(id, cancellationToken);
        return environment != null ? _mapper.Map<EnvironmentDto>(environment) : null;
    }

    public async Task<EnvironmentDto?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var environment = await _environmentRepository.GetByNameAsync(name, cancellationToken);
        return environment != null ? _mapper.Map<EnvironmentDto>(environment) : null;
    }

    public async Task<IEnumerable<EnvironmentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var environments = await _environmentRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<EnvironmentDto>>(environments);
    }

    public async Task<IEnumerable<EnvironmentDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var environments = await _environmentRepository.GetActiveAsync(cancellationToken);
        return _mapper.Map<IEnumerable<EnvironmentDto>>(environments);
    }

    public async Task<IEnumerable<EnvironmentSummaryDto>> GetSummariesAsync(CancellationToken cancellationToken = default)
    {
        var environments = await _environmentRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<EnvironmentSummaryDto>>(environments);
    }

    public async Task<EnvironmentDto> CreateAsync(CreateEnvironmentDto createDto, string createdBy, CancellationToken cancellationToken = default)
    {
        // Check if environment already exists
        var exists = await _environmentRepository.ExistsAsync(createDto.Name, cancellationToken);
        if (exists)
        {
            throw new EnvironmentAlreadyExistsException(createDto.Name);
        }

        var environment = new Domain.Entities.Environment(createDto.Name, createDto.Description, createdBy, createDto.SortOrder);
        await _environmentRepository.AddAsync(environment, cancellationToken);

        _logger.LogInformation("Environment {Name} created by {User}", createDto.Name, createdBy);

        return _mapper.Map<EnvironmentDto>(environment);
    }

    public async Task<EnvironmentDto> UpdateAsync(Guid id, UpdateEnvironmentDto updateDto, string updatedBy, CancellationToken cancellationToken = default)
    {
        var environment = await _environmentRepository.GetByIdAsync(id, cancellationToken);
        if (environment == null)
        {
            throw new EnvironmentNotFoundException(id);
        }

        // Check if new name conflicts with existing environment (if name is being changed)
        if (environment.Name != updateDto.Name)
        {
            var exists = await _environmentRepository.ExistsAsync(updateDto.Name, cancellationToken);
            if (exists)
            {
                throw new EnvironmentAlreadyExistsException(updateDto.Name);
            }
        }

        environment.UpdateDetails(updateDto.Name, updateDto.Description, updatedBy, updateDto.SortOrder);
        await _environmentRepository.UpdateAsync(environment, cancellationToken);

        _logger.LogInformation("Environment {Name} updated by {User}", updateDto.Name, updatedBy);

        return _mapper.Map<EnvironmentDto>(environment);
    }

    public async Task DeleteAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default)
    {
        var environment = await _environmentRepository.GetByIdAsync(id, cancellationToken);
        if (environment == null)
        {
            throw new EnvironmentNotFoundException(id);
        }

        environment.Delete(deletedBy);
        await _environmentRepository.UpdateAsync(environment, cancellationToken);

        _logger.LogInformation("Environment {Name} deleted by {User}", environment.Name, deletedBy);
    }

    public async Task ActivateAsync(Guid id, string updatedBy, CancellationToken cancellationToken = default)
    {
        var environment = await _environmentRepository.GetByIdAsync(id, cancellationToken);
        if (environment == null)
        {
            throw new EnvironmentNotFoundException(id);
        }

        environment.Activate(updatedBy);
        await _environmentRepository.UpdateAsync(environment, cancellationToken);

        _logger.LogInformation("Environment {Name} activated by {User}", environment.Name, updatedBy);
    }

    public async Task DeactivateAsync(Guid id, string updatedBy, CancellationToken cancellationToken = default)
    {
        var environment = await _environmentRepository.GetByIdAsync(id, cancellationToken);
        if (environment == null)
        {
            throw new EnvironmentNotFoundException(id);
        }

        environment.Deactivate(updatedBy);
        await _environmentRepository.UpdateAsync(environment, cancellationToken);

        _logger.LogInformation("Environment {Name} deactivated by {User}", environment.Name, updatedBy);
    }

    public async Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _environmentRepository.ExistsAsync(name, cancellationToken);
    }
}

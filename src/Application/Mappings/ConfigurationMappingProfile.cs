using Application.DTOs.Configuration;
using Application.DTOs.ConfigurationGroup;
using Application.DTOs.Environment;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings;

public class ConfigurationMappingProfile : Profile
{
    public ConfigurationMappingProfile()
    {
        CreateMap<Configuration, ConfigurationDto>()
            .ConstructUsing(src => new ConfigurationDto(
                src.Id,
                src.Key.Value,
                src.Value.Value,
                src.Value.Type,
                src.Description,
                src.ApplicationId,
                src.Application != null ? src.Application.Name : "Unknown",
                src.EnvironmentId,
                src.Environment != null ? src.Environment.Name : "Unknown",
                src.GroupId,
                src.Group != null ? src.Group.Name : null,
                src.IsEncrypted,
                src.IsRequired,
                src.DefaultValue,
                src.IsActive,
                src.Version,
                src.CreatedAt,
                src.UpdatedAt,
                src.CreatedBy,
                src.UpdatedBy));

        CreateMap<ConfigurationHistory, ConfigurationHistoryDto>();

        CreateMap<Domain.Entities.Environment, EnvironmentDto>()
            .ConstructUsing(src => new EnvironmentDto(
                src.Id,
                src.Name,
                src.Description,
                src.IsActive,
                src.SortOrder,
                src.CreatedAt,
                src.UpdatedAt,
                src.CreatedBy,
                src.UpdatedBy,
                src.Configurations.Count));

        CreateMap<Domain.Entities.Environment, EnvironmentSummaryDto>()
            .ConstructUsing(src => new EnvironmentSummaryDto(
                src.Id,
                src.Name,
                src.Description,
                src.IsActive,
                src.Configurations.Count,
                src.UpdatedAt != null ? src.UpdatedAt.Value : src.CreatedAt));

        CreateMap<ConfigurationGroup, ConfigurationGroupDto>()
            .ConstructUsing(src => new ConfigurationGroupDto(
                src.Id,
                src.Name,
                src.Description,
                src.ParentGroupId,
                null, // ParentGroupName - would need proper navigation
                src.IsActive,
                src.SortOrder,
                src.CreatedAt,
                src.UpdatedAt,
                src.CreatedBy,
                src.UpdatedBy,
                src.Configurations.Count,
                src.ChildGroups.Count));

        CreateMap<ConfigurationGroup, ConfigurationGroupTreeDto>()
            .ConstructUsing(src => new ConfigurationGroupTreeDto(
                src.Id,
                src.Name,
                src.Description,
                src.IsActive,
                src.Configurations.Count,
                src.ChildGroups.Select(child => new ConfigurationGroupTreeDto(
                    child.Id,
                    child.Name,
                    child.Description,
                    child.IsActive,
                    child.Configurations.Count,
                    new List<ConfigurationGroupTreeDto>())).ToList()));
    }
}

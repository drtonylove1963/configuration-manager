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
            .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key.Value))
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value.Value))
            .ForMember(dest => dest.ValueType, opt => opt.MapFrom(src => src.Value.Type))
            .ForMember(dest => dest.EnvironmentName, opt => opt.MapFrom(src => src.Environment.Name))
            .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.Group != null ? src.Group.Name : null));

        CreateMap<ConfigurationHistory, ConfigurationHistoryDto>();

        CreateMap<Domain.Entities.Environment, EnvironmentDto>()
            .ForMember(dest => dest.ConfigurationCount, opt => opt.MapFrom(src => src.Configurations.Count));

        CreateMap<Domain.Entities.Environment, EnvironmentSummaryDto>()
            .ForMember(dest => dest.ConfigurationCount, opt => opt.MapFrom(src => src.Configurations.Count))
            .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => src.UpdatedAt ?? src.CreatedAt));

        CreateMap<ConfigurationGroup, ConfigurationGroupDto>()
            .ForMember(dest => dest.ParentGroupName, opt => opt.MapFrom(src => src.ParentGroupId != null ? "Parent Group" : null)) // This would need proper navigation
            .ForMember(dest => dest.ConfigurationCount, opt => opt.MapFrom(src => src.Configurations.Count))
            .ForMember(dest => dest.ChildGroupCount, opt => opt.MapFrom(src => src.ChildGroups.Count));

        CreateMap<ConfigurationGroup, ConfigurationGroupTreeDto>()
            .ForMember(dest => dest.ConfigurationCount, opt => opt.MapFrom(src => src.Configurations.Count))
            .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.ChildGroups));
    }
}

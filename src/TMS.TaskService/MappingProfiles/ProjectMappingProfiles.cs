using AutoMapper;
using TMS.TaskService.Entities;
using TMS.TaskService.Models.Projects;

namespace TMS.TaskService.MappingProfiles;

/// <summary>
/// 
/// </summary>
public class ProjectMappingProfiles : Profile
{
    /// <summary>
    /// 
    /// </summary>
    public ProjectMappingProfiles()
    {
        // Базовый маппинг для всех проектов
        CreateMap<ProjectModelBase, ProjectEntity>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src =>
                string.IsNullOrWhiteSpace(src.Description) ? string.Empty : src.Description.Trim()))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.UserId, opt => opt.Ignore()); // Игнорируем по умолчанию

        // Специфичный маппинг для создания
        CreateMap<ProjectCreate, ProjectEntity>()
            .IncludeBase<ProjectModelBase, ProjectEntity>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

        // Специфичный маппинг для обновления
        CreateMap<ProjectUpdate, ProjectEntity>()
            .IncludeBase<ProjectModelBase, ProjectEntity>();
    }
}
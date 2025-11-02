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
        #region ProjectCreate

        CreateMap<ProjectCreate, ProjectEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(desc => desc.UpdatedAt, opt => opt.Ignore());

        CreateMap<ProjectEntity, ProjectCreate>();

        #endregion ProjectCreate

        #region ProjectUpdate

        CreateMap<ProjectUpdate, ProjectEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(desc => desc.UpdatedAt, opt => opt.Ignore());

        CreateMap<ProjectEntity, ProjectUpdate>();

        #endregion ProjectUpdate
    }
}
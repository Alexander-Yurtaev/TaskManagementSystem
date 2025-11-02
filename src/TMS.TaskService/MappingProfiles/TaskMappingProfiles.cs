using AutoMapper;
using TMS.TaskService.Entities;
using TMS.TaskService.Models.Tasks;

namespace TMS.TaskService.MappingProfiles;

/// <summary>
/// 
/// </summary>
public class TaskMappingProfiles : Profile
{
    /// <summary>
    /// 
    /// </summary>
    public TaskMappingProfiles()
    {
        #region TaskCreate

        CreateMap<TaskCreate, TaskEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<TaskEntity, TaskCreate>();

        #endregion TaskCreate

        #region TaskUpdate

        CreateMap<TaskUpdate, TaskEntity>();

        CreateMap<TaskEntity, TaskUpdate>();

        #endregion TaskUpdate
    }
}
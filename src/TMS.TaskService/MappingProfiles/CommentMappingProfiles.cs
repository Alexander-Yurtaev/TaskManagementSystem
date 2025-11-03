using AutoMapper;
using TMS.TaskService.Entities;
using TMS.TaskService.Models.Comments;

namespace TMS.TaskService.MappingProfiles;

/// <summary>
/// 
/// </summary>
public class CommentMappingProfiles : Profile
{
    /// <summary>
    /// 
    /// </summary>
    public CommentMappingProfiles()
    {
        #region CommentCreate

        CreateMap<CommentCreate, CommentEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<CommentEntity, CommentCreate>();

        #endregion CommentCreate

        #region CommentUpdate

        CreateMap<CommentUpdate, CommentEntity>();

        CreateMap<CommentEntity, CommentUpdate>();

        #endregion CommentUpdate
    }
}
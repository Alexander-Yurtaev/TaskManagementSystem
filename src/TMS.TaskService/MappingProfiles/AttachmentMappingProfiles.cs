using AutoMapper;
using TMS.TaskService.Entities;
using TMS.TaskService.Models.Attachments;

namespace TMS.TaskService.MappingProfiles;

/// <summary>
/// 
/// </summary>
public class AttachmentMappingProfiles : Profile
{
    /// <summary>
    /// 
    /// </summary>
    public AttachmentMappingProfiles()
    {
        #region AttachmentCreate

        CreateMap<AttachmentCreate, AttachmentEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<AttachmentEntity, AttachmentCreate>();

        #endregion AttachmentCreate
    }
}
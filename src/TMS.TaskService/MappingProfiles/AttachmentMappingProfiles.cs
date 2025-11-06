using AutoMapper;
using TMS.Common.Models;
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

        #region AttachmentModel

        CreateMap<AttachmentModel, AttachmentEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<AttachmentEntity, AttachmentModel>();

        #endregion AttachmentModel
    }
}
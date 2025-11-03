using TMS.TaskService.Entities;

namespace TMS.TaskService.Data.Repositories;

/// <summary>
/// 
/// </summary>
public interface IAttachmentRepository
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="attachment"></param>
    /// <returns></returns>
    Task AddAsync(AttachmentEntity attachment);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<AttachmentEntity?> GetByIdAsync(int id);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    Task<IEnumerable<AttachmentEntity>> GetByTaskIdAsync(int taskId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileName"></param>
    /// <param name="taskId"></param>
    /// <returns></returns>
    Task<bool> IsExistsAsync(string filePath, string fileName, int taskId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="attachment"></param>
    /// <returns></returns>
    Task<AttachmentEntity> UpdateAsync(AttachmentEntity attachment);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task DeleteAsync(int id);
}
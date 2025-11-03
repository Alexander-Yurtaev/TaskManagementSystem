using TMS.TaskService.Entities;

namespace TMS.TaskService.Data.Repositories;

/// <summary>
/// 
/// </summary>
public interface ICommentRepository
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="comment"></param>
    /// <returns></returns>
    Task AddAsync(CommentEntity comment);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<CommentEntity?> GetByIdAsync(int id);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    Task<IEnumerable<CommentEntity>> GetByTaskIdAsync(int taskId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    Task<bool> IsExistsAsync(string text);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="comment"></param>
    /// <returns></returns>
    Task<CommentEntity> UpdateAsync(CommentEntity comment);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task DeleteAsync(int id);
}
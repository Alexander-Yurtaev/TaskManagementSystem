using TMS.TaskService.Entities;

namespace TMS.TaskService.Data.Repositories;

/// <summary>
///
/// </summary>
public interface ITaskRepository
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    Task AddAsync(TaskEntity task);

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<TaskEntity>> GetAllAsync();

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<TaskEntity>> GetAllByUserIdAsync(int userId);

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<TaskEntity?> GetByIdAsync(int id);

    /// <summary>
    ///
    /// </summary>
    /// <param name="title"></param>
    /// <returns></returns>
    Task<bool> IsExistsAsync(string title);

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<bool> IsExistsAsync(int id);

    /// <summary>
    ///
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    Task<TaskEntity> UpdateAsync(TaskEntity task);

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task DeleteAsync(int id);
}
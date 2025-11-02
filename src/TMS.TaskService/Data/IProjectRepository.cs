using TMS.TaskService.Entities;

namespace TMS.TaskService.Data;

/// <summary>
/// 
/// </summary>
public interface IProjectRepository
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    Task AddAsync(ProjectEntity task);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<ProjectEntity>> GetAllAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<ProjectEntity?> GetByIdAsync(int id);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="title"></param>
    /// <returns></returns>
    Task<bool> IsExistsAsync(string title);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    Task<ProjectEntity> UpdateAsync(ProjectEntity task);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task DeleteAsync(int id);
}
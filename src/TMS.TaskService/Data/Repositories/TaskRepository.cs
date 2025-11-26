using Microsoft.EntityFrameworkCore;
using TMS.TaskService.Entities;
using TaskStatus = TMS.TaskService.Entities.Enum.TaskStatus;

namespace TMS.TaskService.Data.Repositories;

/// <summary>
///
/// </summary>
/// <param name="db"></param>
public class TaskRepository(TaskDataContext db) : ITaskRepository
{
    private readonly TaskDataContext _db = db;

    /// <summary>
    ///
    /// </summary>
    /// <param name="task"></param>
    public async Task AddAsync(TaskEntity task)
    {
        ArgumentNullException.ThrowIfNull(task, nameof(task));

        await _db.Tasks.AddAsync(task);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<TaskEntity>> GetAllAsync()
    {
        var tasks = await _db.Tasks
            .Where(t => t.Status != TaskStatus.Cancelled)
            .ToArrayAsync();

        return tasks;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<TaskEntity>> GetAllByUserIdAsync(int userId)
    {
        var tasks = await _db.Tasks
            .Where(t => t.Status != TaskStatus.Cancelled && t.AssigneeId == userId)
            .ToArrayAsync();

        return tasks;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<TaskEntity?> GetByIdAsync(int id)
    {
        var task = await _db.Tasks
            .Where(t => t.Id == id && t.Status != TaskStatus.Cancelled)
            .FirstOrDefaultAsync();

        return task;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="title"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public async Task<bool> IsExistsAsync(string title, int projectId)
    {
        ArgumentException.ThrowIfNullOrEmpty(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        return await _db.Tasks.AnyAsync(t => t.Title.ToLower() == title.ToLower() && t.ProjectId == projectId);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<bool> IsExistsAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException(nameof(id));
        }

        return await _db.Tasks.AnyAsync(t => t.Id == id);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="task"></param>
    public async Task<TaskEntity> UpdateAsync(TaskEntity task)
    {
        ArgumentNullException.ThrowIfNull(task, nameof(task));

        _db.Tasks.Update(task);
        await _db.SaveChangesAsync();

        return task;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    public async Task DeleteAsync(int id)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id);
        if (task is null)
        {
            throw new Exception($"Task with id={id} not found.");
        }

        task.Status = TaskStatus.Cancelled;

        await _db.SaveChangesAsync();
    }
}
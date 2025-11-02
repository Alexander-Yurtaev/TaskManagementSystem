using Microsoft.EntityFrameworkCore;
using TMS.TaskService.Entities;

namespace TMS.TaskService.Data;

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
        ArgumentNullException.ThrowIfNull(task);

        await _db.Tasks.AddAsync(task);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<TaskEntity>> GetAllAsync()
    {
        var tasks = await _db.Tasks.ToArrayAsync();
        return tasks;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<TaskEntity?> GetByIdAsync(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        return task;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="title"></param>
    /// <returns></returns>
    public async Task<bool> IsExistsAsync(string title)
    {
        ArgumentException.ThrowIfNullOrEmpty(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        return await _db.Tasks.AnyAsync(t => t.Title.ToLower() == title.ToLower());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="task"></param>
    public async Task<TaskEntity> UpdateAsync(TaskEntity task)
    {
        ArgumentNullException.ThrowIfNull(task);

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

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
    }
}

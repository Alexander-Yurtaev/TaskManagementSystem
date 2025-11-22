using Microsoft.EntityFrameworkCore;
using TMS.TaskService.Entities;
using TMS.TaskService.Entities.Enum;

namespace TMS.TaskService.Data.Repositories;

/// <summary>
///
/// </summary>
/// <param name="db"></param>
public class ProjectRepository(TaskDataContext db) : IProjectRepository
{
    private readonly TaskDataContext _db = db;

    /// <summary>
    ///
    /// </summary>
    /// <param name="project"></param>
    public async Task AddAsync(ProjectEntity project)
    {
        ArgumentNullException.ThrowIfNull(project);

        await _db.Projects.AddAsync(project);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<ProjectEntity>> GetAllAsync()
    {
        var projects = await _db.Projects
            .Where(p => p.Status != ProjectStatus.Cancelled)
            .ToArrayAsync();

        return projects;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ProjectEntity?> GetByIdAsync(int id)
    {
        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == id && p.Status != ProjectStatus.Cancelled);

        return project;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<bool> IsExistsAsync(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return await _db.Projects.AnyAsync(p => p.Name.ToLower() == name.ToLower());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<ProjectEntity>> GetProjectsByUserIdAsync(int userId)
    {
        return await _db.Projects
            .Where(p => p.UserId == userId || p.Tasks.Any(t => t.AssigneeId == userId))
            .AsNoTracking()
            .ToArrayAsync();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<ProjectEntity> UpdateAsync(ProjectEntity project)
    {
        ArgumentNullException.ThrowIfNull(project);

        _db.Projects.Update(project);
        await _db.SaveChangesAsync();

        return project;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    public async Task DeleteAsync(int id)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(t => t.Id == id);
        if (project is null)
        {
            throw new KeyNotFoundException($"Project with id={id} not found.");
        }

        project.Status = ProjectStatus.Cancelled;

        await _db.SaveChangesAsync();
    }
}
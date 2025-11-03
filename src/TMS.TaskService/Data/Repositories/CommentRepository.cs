using Microsoft.EntityFrameworkCore;
using TMS.TaskService.Entities;

namespace TMS.TaskService.Data.Repositories;

/// <summary>
/// 
/// </summary>
/// <param name="db"></param>
public class CommentRepository(TaskDataContext db) : ICommentRepository
{
    private readonly TaskDataContext _db = db;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="comment"></param>
    public async Task AddAsync(CommentEntity comment)
    {
        ArgumentNullException.ThrowIfNull(comment);

        await _db.Comments.AddAsync(comment);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<CommentEntity?> GetByIdAsync(int id)
    {
        var comment = await _db.Comments.FindAsync(id);
        return comment;
    }

    /// <summary>
    /// 
    /// </summary>
    /// /// <param name="taskId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<CommentEntity>> GetByTaskIdAsync(int taskId)
    {
        var comments = await _db.Comments
            .Where(c => c.TaskId == taskId)
            .ToArrayAsync();

        return comments;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public async Task<bool> IsExistsAsync(string text)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);

        return await _db.Comments.AnyAsync(a => a.Text == text);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="comment"></param>
    public async Task<CommentEntity> UpdateAsync(CommentEntity comment)
    {
        ArgumentNullException.ThrowIfNull(comment);

        _db.Comments.Update(comment);
        await _db.SaveChangesAsync();

        return comment;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public async Task DeleteAsync(int id)
    {
        var comment = await _db.Comments.FirstOrDefaultAsync(t => t.Id == id);
        if (comment is null)
        {
            throw new Exception($"Comment with id={id} not found.");
        }

        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();
    }
}

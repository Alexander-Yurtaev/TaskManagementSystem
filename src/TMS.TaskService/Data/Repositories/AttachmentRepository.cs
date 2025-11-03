using Microsoft.EntityFrameworkCore;
using TMS.TaskService.Entities;

namespace TMS.TaskService.Data.Repositories;

/// <summary>
/// 
/// </summary>
/// <param name="db"></param>
public class AttachmentRepository(TaskDataContext db) : IAttachmentRepository
{
    private readonly TaskDataContext _db = db;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="attachment"></param>
    public async Task AddAsync(AttachmentEntity attachment)
    {
        ArgumentNullException.ThrowIfNull(attachment);

        await _db.Attachments.AddAsync(attachment);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<AttachmentEntity>> GetAllAsync()
    {
        var attachments = await _db.Attachments.ToArrayAsync();
        return attachments;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<AttachmentEntity?> GetByIdAsync(int id)
    {
        var attachment = await _db.Attachments.FindAsync(id);
        return attachment;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileName"></param>
    /// <param name="taskId"></param>
    /// <returns></returns>
    public async Task<bool> IsExistsAsync(string filePath, string fileName, int taskId)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        return await _db.Attachments.AnyAsync(t => t.FilePath.ToLower() == filePath.ToLower() &&
                                                   t.FileName.ToLower() == fileName.ToLower() &&
                                                   t.Id == taskId);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="attachment"></param>
    public async Task<AttachmentEntity> UpdateAsync(AttachmentEntity attachment)
    {
        ArgumentNullException.ThrowIfNull(attachment);

        _db.Attachments.Update(attachment);
        await _db.SaveChangesAsync();

        return attachment;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public async Task DeleteAsync(int id)
    {
        var attachment = await _db.Attachments.FirstOrDefaultAsync(t => t.Id == id);
        if (attachment is null)
        {
            throw new Exception($"Attachment with id={id} not found.");
        }

        _db.Attachments.Remove(attachment);
        await _db.SaveChangesAsync();
    }
}

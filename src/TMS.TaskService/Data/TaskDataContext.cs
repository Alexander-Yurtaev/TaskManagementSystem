using Microsoft.EntityFrameworkCore;
using TMS.TaskService.Entities;

namespace TMS.TaskService.Data;

/// <summary>
/// 
/// </summary>
public class TaskDataContext : DbContext
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    public TaskDataContext(DbContextOptions<TaskDataContext> options) : base(options)
    {
        
    }

    /// <summary>
    /// 
    /// </summary>
    public DbSet<ProjectEntity> Projects { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public DbSet<TaskEntity> Tasks { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public DbSet<CommentEntity> Comments { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public DbSet<AttachmentEntity> Attachments { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Project - Task (1:N)
        modelBuilder.Entity<ProjectEntity>()
            .HasMany(p => p.Tasks)
            .WithOne(t => t.Project);

        // Task - Comment (1:N)
        modelBuilder.Entity<TaskEntity>()
            .HasMany(t => t.Comments)
            .WithOne(t => t.Task);

        // Task - Attachment (1:N)
        modelBuilder.Entity<TaskEntity>()
            .HasMany(t => t.Attachments)
            .WithOne(a => a.Task);

        modelBuilder.Entity<AttachmentEntity>()
            .HasOne(n => n.Task)
            .WithMany(t => t.Attachments)
            .HasForeignKey(n => n.TaskId)
            .IsRequired(); // явно указываем обязательность связи

        modelBuilder.Entity<CommentEntity>()
            .HasOne(n => n.Task)
            .WithMany(t => t.Comments)
            .HasForeignKey(n => n.TaskId)
            .IsRequired(); // явно указываем обязательность связи

        modelBuilder.Entity<TaskEntity>()
            .HasOne(n => n.Project)
            .WithMany(t => t.Tasks)
            .HasForeignKey(n => n.ProjectId)
            .IsRequired(); // явно указываем обязательность связи
    }
}
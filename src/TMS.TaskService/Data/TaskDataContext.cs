using Microsoft.EntityFrameworkCore;
using TMS.Entities.Task;

namespace TMS.TaskService.Data;

public class TaskDataContext : DbContext
{
    public TaskDataContext(DbContextOptions options) : base(options)
    {
        
    }

    public DbSet<ProjectEntity> Projects { get; set; }

    public DbSet<TaskEntity> Tasks { get; set; }

    public DbSet<CommentEntity> Comments { get; set; }

    public DbSet<AttachmentEntity> Attachments { get; set; }

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
    }
}
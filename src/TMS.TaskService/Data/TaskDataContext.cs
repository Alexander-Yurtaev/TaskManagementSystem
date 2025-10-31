using Microsoft.EntityFrameworkCore;
using TMS.Entities.Notification;
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

        modelBuilder.Entity<CommentEntity>()
            .HasOne(n => n.User)
            .WithMany(t => t.Comments)
            .HasForeignKey(n => n.UserId)
            .IsRequired(); // явно указываем обязательность связи

        modelBuilder.Entity<ProjectEntity>()
            .HasOne(n => n.User)
            .WithMany(t => t.Projects)
            .HasForeignKey(n => n.UserId)
            .IsRequired(); // явно указываем обязательность связи

        modelBuilder.Entity<TaskEntity>()
            .HasOne(n => n.Project)
            .WithMany(t => t.Tasks)
            .HasForeignKey(n => n.ProjectId)
            .IsRequired(); // явно указываем обязательность связи
    }
}
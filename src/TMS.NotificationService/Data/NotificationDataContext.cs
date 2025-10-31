using Microsoft.EntityFrameworkCore;
using TMS.Entities.Notification;

namespace TMS.NotificationService.Data;

public class NotificationDataContext : DbContext
{
    public NotificationDataContext(DbContextOptions options) : base(options)
    {
        
    }

    public DbSet<NotificationEntity> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationEntity>()
            .HasOne(n => n.Task)
            .WithMany(t => t.Notifications)
            .HasForeignKey(n => n.TaskId)
            .IsRequired(); // явно указываем обязательность связи
    }
}
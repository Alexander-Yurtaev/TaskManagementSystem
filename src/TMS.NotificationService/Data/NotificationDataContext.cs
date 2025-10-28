using Microsoft.EntityFrameworkCore;
using TMS.Entities.Notification;

namespace TMS.NotificationService.Data;

public class NotificationDataContext : DbContext
{
    public NotificationDataContext(DbContextOptions options) : base(options)
    {
        
    }

    public DbSet<NotificationEntity> Notifications { get; set; }
}
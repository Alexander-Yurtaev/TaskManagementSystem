using Microsoft.EntityFrameworkCore;
using TMS.Entities.Notification;

namespace TMS.NotificationService.Data;

public class NotificationDataContext : DbContext
{
    public DbSet<NotificationEntity> Notifications { get; set; }
}
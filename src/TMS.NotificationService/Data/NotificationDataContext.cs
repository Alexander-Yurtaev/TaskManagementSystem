using Microsoft.EntityFrameworkCore;
using TMS.NotificationService.Entities;

namespace TMS.NotificationService.Data;

public class NotificationDataContext(DbContextOptions<NotificationDataContext> options) : DbContext(options)
{
    public DbSet<NotificationEntity> Notifications { get; set; }
}
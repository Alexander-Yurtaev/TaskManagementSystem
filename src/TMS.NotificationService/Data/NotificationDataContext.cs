using Microsoft.EntityFrameworkCore;
using TMS.NotificationService.Entities;

namespace TMS.NotificationService.Data;

public class NotificationDataContext : DbContext
{
    public NotificationDataContext(DbContextOptions<NotificationDataContext> options) : base(options)
    {

    }

    public DbSet<NotificationEntity> Notifications { get; set; }
}
using Microsoft.EntityFrameworkCore;
using TMS.NotificationService.Entities;

namespace TMS.NotificationService.Data;

/// <summary>
/// 
/// </summary>
/// <param name="options"></param>
public class NotificationDataContext(DbContextOptions<NotificationDataContext> options) : DbContext(options)
{
    /// <summary>
    /// 
    /// </summary>
    public DbSet<NotificationEntity> Notifications { get; set; }
}
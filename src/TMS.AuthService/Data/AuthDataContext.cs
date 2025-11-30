using Microsoft.EntityFrameworkCore;
using TMS.AuthService.Entities;
using TMS.AuthService.Entities.Enum;

namespace TMS.AuthService.Data;

/// <summary>
///
/// </summary>
public class AuthDataContext : DbContext
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="options"></param>
    public AuthDataContext(DbContextOptions<AuthDataContext> options) : base(options)
    {
    }

    /// <summary>
    ///
    /// </summary>
    public DbSet<UserEntity> Users { get; set; }

    /// <summary>
    ///
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>()
            .Property(u => u.Role)
            .HasDefaultValue(UserRole.User);

        modelBuilder.Entity<UserEntity>()
            .Property(e => e.CreateAt)
            .HasColumnType("timestamp with time zone") // Явно задаём тип TIMESTAMPTZ
            .HasDefaultValueSql("timezone('UTC', NOW())") // Дефолт: NOW() в UTC
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<UserEntity>()
            .Property(e => e.UpdatedAt)
            .HasColumnType("timestamp with time zone") // Явно задаём тип TIMESTAMPTZ
            .HasDefaultValueSql("timezone('UTC', NOW())") // Дефолт: NOW() в UTC
            .ValueGeneratedOnAddOrUpdate();

        modelBuilder.Entity<UserEntity>()
            .Property(e => e.DeletedAt)
            .HasColumnType("timestamp with time zone"); // Явно задаём тип TIMESTAMPTZ

        modelBuilder.Entity<UserEntity>().HasData(
            new UserEntity
            {
                Id = 1,
                UserName = "superadmin",
                Email = "superadmin@tms.ru",
                PasswordHash = "$2b$12$b6doeIcInzPKmTANJeO7Euc3UE.VRkVLSY2E3gKLHbUvRLM4Y7NrS",
                Role = UserRole.SuperAdmin
            });
    }
}
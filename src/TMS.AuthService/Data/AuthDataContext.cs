using Microsoft.EntityFrameworkCore;
using TMS.Entities.Auth;

namespace TMS.AuthService.Data;

public class AuthDataContext : DbContext
{
    public AuthDataContext() : base()
    {
        
    }

    public AuthDataContext(DbContextOptions options) : base(options)
    {
        
    }

    public DbSet<UserEntity> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
    }
}
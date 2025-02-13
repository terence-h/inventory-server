using inventory_server.Entities;
using Microsoft.EntityFrameworkCore;

namespace inventory_server.Database;

public class AuditTypeDbContext(DbContextOptions<AuditTypeDbContext> options) : DbContext(options)
{
    public DbSet<AuditType> AuditTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AuditType>(entity =>
        {
            entity.HasKey(e => e.AuditTypeId);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(e => e.Name)
                .IsUnique();
            
            entity.HasMany(e => e.AuditLogs)
                .WithOne(e => e.AuditType)
                .HasForeignKey(e => e.AuditTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
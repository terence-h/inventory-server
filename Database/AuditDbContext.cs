using inventory_server.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace inventory_server.Database;

public class AuditDbContext(DbContextOptions<AuditDbContext> options) : DbContext(options)
{
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId);
            
            entity.Property(e => e.AuditLogId)
                .HasValueGenerator(typeof(SequentialGuidValueGenerator));

            entity.Property(e => e.AuditTypeId)
                .IsRequired();
            
            entity.Property(e => e.AuditContent)
                .IsRequired();
            
            entity.Property(e => e.ActionBy)
                .IsRequired();
            
            entity.Property(e => e.Date)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => e.AuditLogId)
                .IsUnique();
        });
    }
}
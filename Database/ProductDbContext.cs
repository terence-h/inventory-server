using inventory_server.Models;
using Microsoft.EntityFrameworkCore;

namespace inventory_server.Database;

public class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId);

            entity.Property(e => e.BatchNo)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Quantity)
                .IsRequired();

            entity.Property(e => e.MfgDate)
                .IsRequired();

            entity.Property(e => e.MfgExpiryDate);
        });
    }
}
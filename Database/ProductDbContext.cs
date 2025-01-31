using inventory_server.Entities;
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

            entity.Property(e => e.ProductId)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.ProductNo)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ProductName)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Manufacturer)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.BatchNo)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Quantity)
                .IsRequired();
            
            entity.Property(e => e.CategoryId)
                .IsRequired();

            entity.Property(e => e.MfgDate);

            entity.Property(e => e.MfgExpiryDate);

            entity.Property(e => e.AddedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.RowVersion)
                .HasDefaultValue(1)
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        });
    }
}
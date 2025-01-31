using inventory_server.Entities;
using Microsoft.EntityFrameworkCore;

namespace inventory_server.Database;

public class CategoryDbContext(DbContextOptions<CategoryDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId);
            
            entity.Property(e => e.CategoryId)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.CategoryName)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(e => e.CategoryName)
                .IsUnique();
            
            entity.HasMany(e => e.Products)
                .WithOne(e => e.Category)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
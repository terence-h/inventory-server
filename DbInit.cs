using inventory_server.Database;
using inventory_server.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace inventory_server;

public static class DbInit
{
    public static async Task InitialiseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var accountDbContext = services.GetRequiredService<AccountDbContext>();
            var productDbContext = services.GetRequiredService<ProductDbContext>();
            var auditDbContext = services.GetRequiredService<AuditDbContext>();
            
            var accountPendingMigrations  = await accountDbContext.Database.GetPendingMigrationsAsync();
            var productPendingMigrations  = await productDbContext.Database.GetPendingMigrationsAsync();
            var auditPendingMigrations  = await auditDbContext.Database.GetPendingMigrationsAsync();

            if (accountPendingMigrations.Any() || productPendingMigrations.Any() || auditPendingMigrations.Any())
            {
                await accountDbContext.Database.MigrateAsync();
                await productDbContext.Database.MigrateAsync(); // ProductDbContext will create category db
                await auditDbContext.Database.MigrateAsync(); // AuditDbContext will create types db
                
                await SeedUsersAsync(services);
                
                var categoryDbContext = services.GetRequiredService<CategoryDbContext>();
                await SeedCategoriesAsync(categoryDbContext);
                
                var auditTypesDbContext = services.GetRequiredService<AuditTypeDbContext>();
                await SeedAuditTypes(auditTypesDbContext);
            }
            else
            {
                Console.WriteLine("Database is already up-to-date. Skipping migration and seeding.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database initialisation failed: {ex.Message}");
        }
    }

    private static async Task SeedUsersAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<Account>>();
        
        if (await userManager.FindByNameAsync("admin") == null)
        {
            var admin = new Account
            {
                Id = "3ac9c24d-39b4-468b-9fd0-c4dfcbb9e0ef",
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                SecurityStamp = "static-stamp-1"
            };
            await userManager.CreateAsync(admin, "password123");
        }

        if (await userManager.FindByNameAsync("user") == null)
        {
            var user = new Account
            {
                Id = "4a0afd45-eadb-4e4a-8d24-de54fa779b28",
                UserName = "user",
                NormalizedUserName = "USER",
                SecurityStamp = "static-stamp-2"
            };
            await userManager.CreateAsync(user, "password123");
        }
    }

    private static async Task SeedCategoriesAsync(CategoryDbContext dbContext)
    {
        if (!await dbContext.Categories.AnyAsync())
        {
            dbContext.Categories.AddRange(
                new Category { CategoryId = 1, CategoryName = "Fresh Produce" },
                new Category { CategoryId = 2, CategoryName = "Meat & Seafood" },
                new Category { CategoryId = 3, CategoryName = "Beverages" },
                new Category { CategoryId = 4, CategoryName = "Household & Kitchen Essentials" },
                new Category { CategoryId = 5, CategoryName = "Others" }
            );
            await dbContext.SaveChangesAsync();
        }
    }

    private static async Task SeedAuditTypes(AuditTypeDbContext dbContext)
    {
        if (!await dbContext.AuditTypes.AnyAsync())
        {
            dbContext.AuditTypes.AddRange(
                new AuditType { AuditTypeId = 1, Name = "Register" },
                new AuditType { AuditTypeId = 2, Name = "Login" },
                new AuditType { AuditTypeId = 3, Name = "Logout" },
                new AuditType { AuditTypeId = 4, Name = "Add Product" },
                new AuditType { AuditTypeId = 5, Name = "Edit Product" },
                new AuditType { AuditTypeId = 6, Name = "Delete Product" }
            );
            await dbContext.SaveChangesAsync();
        }
    }
}
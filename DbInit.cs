using System.Globalization;
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

                await SeedProductsAsync(productDbContext);
                
                var auditTypesDbContext = services.GetRequiredService<AuditTypeDbContext>();
                await SeedAuditTypes(auditTypesDbContext);
                
                await SeedAuditLogs(auditDbContext);
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
                new Category { CategoryId = 4, CategoryName = "Household Essentials" },
                new Category { CategoryId = 5, CategoryName = "Others" }
            );
            await dbContext.SaveChangesAsync();
        }
    }

    private static async Task SeedAuditLogs(AuditDbContext dbContext)
    {
        if (!await dbContext.AuditLogs.AnyAsync())
        {
            var random = new Random();
            DateTime baseDate = DateTime.Parse("2024-03-20 14:21:48.677164+08", null, DateTimeStyles.AdjustToUniversal);
            
            dbContext.AuditLogs.Add(new AuditLog
            {
                AuditLogId = Guid.Parse("0849c853-2d22-42b3-6629-08dd51766262"),
                AuditTypeId = 4,
                AuditContent =
                    $"[AddProductSuccess]ProductId:1,ProductNo:BEV-001,ProductName:Cold Brew Coffee,Manufacturer:Brew Masters,BatchNo:B2023-06,Quantity:427,CategoryId:5,MfgDate:11/09/2024 12:00:00 am,MfgExpiryDate:26/10/2025 12:00:00 am",
                ActionBy = "admin",
                Date = baseDate
            });

            for (int i = 0; i < 12; i++)
            {
                DateTime dateValue = baseDate.AddMonths(i);
                int randomQuantity = random.Next(10, 1001);
                
                string auditContent = $"[EditProductSuccess]ProductId:1,ProductNo:BEV-001,ProductName:Cold Brew Coffee,Manufacturer:Brew Masters,BatchNo:B2023-06,Quantity:{randomQuantity},CategoryId:5,MfgDate:11/09/2024 12:00:00 am,MfgExpiryDate:26/10/2025 12:00:00 am";

                var auditLog = new AuditLog
                {
                    AuditLogId = Guid.NewGuid(),
                    AuditTypeId = 5,
                    AuditContent = auditContent,
                    ActionBy = "admin",
                    Date = dateValue
                };

                dbContext.AuditLogs.Add(auditLog);
            }
            
            await dbContext.SaveChangesAsync();
        }
    }

    private static async Task SeedProductsAsync(ProductDbContext dbContext)
    {
        if (!await dbContext.Products.AnyAsync())
        {
            dbContext.Products.AddRange(
                new Product
                {
                    ProductId = 1,
                    ProductNo = "BEV-001",
                    ProductName = "Cold Brew Coffee",
                    Manufacturer = "Brew Masters",
                    BatchNo = "B2023-06",
                    Quantity = 427,
                    CategoryId = 5,
                    MfgDate = DateTime.SpecifyKind(DateTime.Parse("2024-09-11"), DateTimeKind.Utc),
                    MfgExpiryDate = DateTime.SpecifyKind(DateTime.Parse("2025-10-26"), DateTimeKind.Utc)
                },
                new Product
                {
                    ProductId = 2,
                    ProductNo = "HOUSE-001",
                    ProductName = "Dish Soap",
                    Manufacturer = "Clean & Green",
                    BatchNo = "B2023-09",
                    Quantity = 118,
                    CategoryId = 4,
                    MfgDate = DateTime.SpecifyKind(DateTime.Parse("2024-02-24"), DateTimeKind.Utc),
                    MfgExpiryDate = null
                },
                new Product
                {
                    ProductId = 3,
                    ProductNo = "HOUSE-002",
                    ProductName = "Laundry Detergent",
                    Manufacturer = "Fresh Scent",
                    BatchNo = "B2024-01",
                    Quantity = 59,
                    CategoryId = 5,
                    MfgDate = null,
                    MfgExpiryDate = null
                },
                new Product
                {
                    ProductId = 4,
                    ProductNo = "FRUIT-002",
                    ProductName = "Banana",
                    Manufacturer = "Tropical Fruits Co.",
                    BatchNo = "B2024-09",
                    Quantity = 199,
                    CategoryId = 1,
                    MfgDate = DateTime.SpecifyKind(DateTime.Parse("2024-10-21"), DateTimeKind.Utc),
                    MfgExpiryDate = DateTime.SpecifyKind(DateTime.Parse("2025-11-06"), DateTimeKind.Utc)
                },
                new Product
                {
                    ProductId = 5,
                    ProductNo = "SEAFOOD-001",
                    ProductName = "Fresh Salmon",
                    Manufacturer = "Ocean Catch",
                    BatchNo = "B2025-06",
                    Quantity = 139,
                    CategoryId = 5,
                    MfgDate = DateTime.SpecifyKind(DateTime.Parse("2024-12-14"), DateTimeKind.Utc),
                    MfgExpiryDate = DateTime.SpecifyKind(DateTime.Parse("2025-04-16"), DateTimeKind.Utc)
                },
                new Product
                {
                    ProductId = 6,
                    ProductNo = "OTHER-001",
                    ProductName = "Organic Honey",
                    Manufacturer = "Golden Farms",
                    BatchNo = "B2024-02",
                    Quantity = 426,
                    CategoryId = 4,
                    MfgDate = DateTime.SpecifyKind(DateTime.Parse("2024-09-23"), DateTimeKind.Utc),
                    MfgExpiryDate = DateTime.SpecifyKind(DateTime.Parse("2025-10-20"), DateTimeKind.Utc)
                },
                new Product
                {
                    ProductId = 7,
                    ProductNo = "FRUIT-002",
                    ProductName = "Banana",
                    Manufacturer = "Tropical Fruits Co.",
                    BatchNo = "B2023-07",
                    Quantity = 380,
                    CategoryId = 5,
                    MfgDate = DateTime.SpecifyKind(DateTime.Parse("2024-03-29"), DateTimeKind.Utc),
                    MfgExpiryDate = DateTime.SpecifyKind(DateTime.Parse("2025-08-19"), DateTimeKind.Utc)
                },
                new Product
                {
                    ProductId = 8,
                    ProductNo = "SEAFOOD-001",
                    ProductName = "Fresh Salmon",
                    Manufacturer = "Ocean Catch",
                    BatchNo = "B2024-11",
                    Quantity = 392,
                    CategoryId = 3,
                    MfgDate = DateTime.SpecifyKind(DateTime.Parse("2024-11-22"), DateTimeKind.Utc),
                    MfgExpiryDate = DateTime.SpecifyKind(DateTime.Parse("2025-10-06"), DateTimeKind.Utc)
                },
                new Product
                {
                    ProductId = 9,
                    ProductNo = "SEAFOOD-001",
                    ProductName = "Fresh Salmon",
                    Manufacturer = "Ocean Catch",
                    BatchNo = "B2024-04",
                    Quantity = 493,
                    CategoryId = 5,
                    MfgDate = DateTime.SpecifyKind(DateTime.Parse("2024-11-17"), DateTimeKind.Utc),
                    MfgExpiryDate = DateTime.SpecifyKind(DateTime.Parse("2025-08-08"), DateTimeKind.Utc)
                },
                new Product
                {
                    ProductId = 10,
                    ProductNo = "BEV-001",
                    ProductName = "Cold Brew Coffee",
                    Manufacturer = "Brew Masters",
                    BatchNo = "B2023-11",
                    Quantity = 153,
                    CategoryId = 4,
                    MfgDate = DateTime.SpecifyKind(DateTime.Parse("2024-12-05"), DateTimeKind.Utc),
                    MfgExpiryDate = DateTime.SpecifyKind(DateTime.Parse("2025-05-03"), DateTimeKind.Utc)
                },
                new Product
                {
                    ProductId = 11,
                    ProductNo = "OTHER-001",
                    ProductName = "Organic Honey",
                    Manufacturer = "Golden Farms",
                    BatchNo = "B2023-12",
                    Quantity = 483,
                    CategoryId = 2,
                    MfgDate = DateTime.SpecifyKind(DateTime.Parse("2024-10-17"), DateTimeKind.Utc),
                    MfgExpiryDate = DateTime.SpecifyKind(DateTime.Parse("2025-09-25"), DateTimeKind.Utc)
                },
                new Product
                {
                    ProductId = 12,
                    ProductNo = "MEAT-001",
                    ProductName = "Grass-Fed Beef",
                    Manufacturer = "Prime Meats",
                    BatchNo = "B2024-07",
                    Quantity = 474,
                    CategoryId = 4,
                    MfgDate = DateTime.SpecifyKind(DateTime.Parse("2025-01-19"), DateTimeKind.Utc),
                    MfgExpiryDate = DateTime.SpecifyKind(DateTime.Parse("2025-04-27"), DateTimeKind.Utc)
                },
                new Product
                {
                    ProductId = 13,
                    ProductNo = "HOUSE-001",
                    ProductName = "Dish Soap",
                    Manufacturer = "Clean & Green",
                    BatchNo = "B2025-10",
                    Quantity = 330,
                    CategoryId = 5,
                    MfgDate = DateTime.SpecifyKind(DateTime.Parse("2024-05-19"), DateTimeKind.Utc),
                    MfgExpiryDate = DateTime.SpecifyKind(DateTime.Parse("2025-11-29"), DateTimeKind.Utc)
                },
                new Product
                {
                    ProductId = 14,
                    ProductNo = "OTHER-001",
                    ProductName = "Organic Honey",
                    Manufacturer = "Golden Farms",
                    BatchNo = "B2025-02",
                    Quantity = 350,
                    CategoryId = 4,
                    MfgDate = DateTime.SpecifyKind(DateTime.Parse("2024-06-16"), DateTimeKind.Utc),
                    MfgExpiryDate = DateTime.SpecifyKind(DateTime.Parse("2025-06-26"), DateTimeKind.Utc)
                },
                new Product
                {
                    ProductId = 15,
                    ProductNo = "OTHER-001",
                    ProductName = "Organic Honey",
                    Manufacturer = "Golden Farms",
                    BatchNo = "B2025-04",
                    Quantity = 130,
                    CategoryId = 5,
                    MfgDate = DateTime.SpecifyKind(DateTime.Parse("2024-10-03"), DateTimeKind.Utc),
                    MfgExpiryDate = DateTime.SpecifyKind(DateTime.Parse("2025-10-20"), DateTimeKind.Utc)
                }
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
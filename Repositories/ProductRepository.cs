using AutoMapper;
using inventory_server.Database;
using inventory_server.Entities;
using inventory_server.Models.Requests;
using inventory_server.Models.Response;
using inventory_server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace inventory_server.Repositories;

public class ProductRepository(ProductDbContext context, IMapper mapper) : IProductRepository
{

    public async Task<IEnumerable<Product>> GetProductsAsync(GetProductsRequest filters)
    {
        IQueryable<Product> products = context.Products
            .Include(p => p.Category);
        
        if (!string.IsNullOrEmpty(filters.ProductNo))
        {
            products = products.Where(p => p.ProductNo.Contains(filters.ProductNo));
        }

        if (!string.IsNullOrEmpty(filters.ProductName))
        {
            products = products.Where(p => p.ProductName.Contains(filters.ProductName));
        }

        if (!string.IsNullOrEmpty(filters.Manufacturer))
        {
            products = products.Where(p => p.Manufacturer.Contains(filters.Manufacturer));
        }
        
        if (!string.IsNullOrEmpty(filters.BatchNo))
        {
            products = products.Where(p => p.Manufacturer.Contains(filters.BatchNo));
        }
        
        if (filters.Quantity.HasValue)
        {
            products = products.Where(p => p.Quantity >= filters.Quantity);
        }
        
        if (filters.CategoryId.HasValue)
        {
            products = products.Where(p => p.CategoryId == filters.CategoryId);
        }

        if (filters.MfgDateFrom.HasValue || filters.MfgDateTo.HasValue)
        {
            products = products.Where(p => p.MfgDate >= (filters.MfgDateFrom ?? DateTime.UnixEpoch) && p.MfgDate <= (filters.MfgDateTo ?? DateTime.MaxValue));
        }
        
        if (filters.MfgExpiryDateFrom.HasValue || filters.MfgExpiryDateTo.HasValue)
        {
            products = products.Where(p => p.MfgDate >= (filters.MfgExpiryDateFrom ?? DateTime.UnixEpoch) && p.MfgDate <= (filters.MfgExpiryDateTo ?? DateTime.MaxValue));
        }
        
        return await products.ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        var product = await context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.ProductId == id);
        
        return product ?? null;
    }

    public async Task<AddProductResponse> AddProductAsync(AddProductRequest request)
    {
        var product = mapper.Map<Product>(request);
        
        try
        {
            context.Products.Add(product);

            var created = await context.SaveChangesAsync();

            if (created > 0)
            {
                return new AddProductResponse
                    { ProductId = product.ProductId, Message = $"{product.ProductName} added successfully." };
            }

            return new AddProductResponse
                { ProductId = 0, Message = $"Error adding {product.ProductName}." };
        }
        catch (Exception ex)
        {
            return new AddProductResponse { ProductId = 0, Message = $"ERROR AddProductAsync: {ex.Message}" };
        }
    }

    public async Task UpdateProductAsync(Product product)
    {
        context.Products.Update(product);
        await context.SaveChangesAsync();
    }

    public async Task DeleteProductAsync(int id)
    {
        var product = await context.Products.FindAsync(id);
        
        if (product != null)
        {
            context.Products.Remove(product);
            await context.SaveChangesAsync();
        }
    }
}